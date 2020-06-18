using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using System.IO;
using UnityEngine.Events;
using Unity.Jobs;
using UnityEditor;

public class ServerBehaviour : MonoBehaviour
{

    private NetworkDriver networkDriver;

    private ServerDataHolder serverDataHolder;

    private NativeList<NetworkConnection> connections;

    private JobHandle networkJobHandle;

    public Queue<MessageHeader> messagesQueue;

    public UnityEvent<MessageHeader>[] ServerCallbacks = new UnityEvent<MessageHeader>[(int)MessageHeader.MessageType.count - 1];

    // Start is called before the first frame update
    void Start()
    {
        serverDataHolder = new ServerDataHolder();

        networkDriver = NetworkDriver.Create();
        NetworkEndPoint endPoint = NetworkEndPoint.AnyIpv4; 
        endPoint.Port = 9000;

        if (networkDriver.Bind(endPoint) != 0)
        {
            Debug.Log("Failed to bind port to" + endPoint.Port);
        } else
        {
            networkDriver.Listen();
        }

        connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        messagesQueue = new Queue<MessageHeader>();

        for (int i = 0; i < ServerCallbacks.Length; i++)
        {
            ServerCallbacks[i] = new MessageEvent();
        }
    }


    void Update()
    {
        networkJobHandle.Complete();

        for (int i = 0; i < connections.Length; ++i)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        NetworkConnection newConnection;
        while ((newConnection = networkDriver.Accept()) != default)
        {
            // new connection
            connections.Add(newConnection);
            Color col = ColorExtensions.colors[ (ColorExtensions.RandomStartIndex + newConnection.InternalId) % ColorExtensions.colors.Length];
            col.a = 1;
            var colour =(Color32)col;   
            var playerID = newConnection.InternalId;
            // welcomes new player
            var welcomeMessage = new WelcomeMessage
            {
                PlayerID = playerID,
                Colour = ((uint)colour.r << 24) | ((uint)colour.g << 16) | ((uint)colour.b << 8) | colour.a
            };
            SendMessage(welcomeMessage, newConnection);

            //sets player data
            PlayerSettings data = new PlayerSettings();
            data.playerColor = colour;
            data.playerID = playerID;
            if (serverDataHolder.players == null) { serverDataHolder.players = new List<PlayerSettings>(); }
            serverDataHolder.players.Add(data);

        }

        PlayerSettings playerData;
        RoomSettings currentRoom;
        Monster monster;
        DataStreamReader reader;
        for (int i = 0; i < connections.Length; ++i)
        {
            if (!connections[i].IsCreated) continue;

            NetworkEvent.Type cmd;
            while ((cmd = networkDriver.PopEventForConnection(connections[i], out reader)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    var messageType = (MessageHeader.MessageType)reader.ReadUShort();
                    switch (messageType)
                    {
                        case MessageHeader.MessageType.none:
                            StayAlive(i);
                            break;
                        case MessageHeader.MessageType.setName:
                            // sets player name
                            var message = new SetNameMessage();
                            message.DeserializeObject(ref reader);
                            messagesQueue.Enqueue(message);

                            PlayerSettings newPlayerData = GetPlayerData(connections[i]);
                            newPlayerData.name = message.Name;

                            NewPlayerJoined(connections[i]);

                            break;
                        case MessageHeader.MessageType.moveRequest:
                            // player move request
                            var moveRequest = new MoverequestMessage();
                            moveRequest.DeserializeObject(ref reader);
                            bool canmove = TryMoveRequest(moveRequest, i);
                            if (canmove)
                            {
                                // calls next turn function on all joined instances.
                                NextTurn();
                            }
                            break;
                        case MessageHeader.MessageType.claimTreasureRequest:
                            //Claim treasure request
                            var treasureRquest = new ClaimTreasureRequestMessage();
                            treasureRquest.DeserializeObject(ref reader);
                            playerData = serverDataHolder.players.Find(x => x.playerID == i);
                            currentRoom = serverDataHolder.rooms[playerData.roomID[0], playerData.roomID[1]];

                            if (currentRoom.treasureAmmount <= 0)
                            {
                                RequestDenied(treasureRquest, i);
                                return;
                            }

                            //update treasrure
                            int gainedTreasure = currentRoom.treasureAmmount;
                            currentRoom.treasureAmmount = 0;

                            //send obtain message back to all players in room
                            List<int> ids = serverDataHolder.GetOtherPlayersInRoom(playerData);
                            ids.Add(i);
                            ObtainTreasureMessage obtainMessage = new ObtainTreasureMessage()
                            {
                                Amount = (ushort)(gainedTreasure / ids.Count)
                            };
                            foreach (int id in ids)
                            {
                                serverDataHolder.players.Find(x => x.playerID == id).score += gainedTreasure / ids.Count;
                                SendMessage(obtainMessage, connections[serverDataHolder.players.Find(x => x.playerID == id).playerID]);
                            }
                            NextTurn();
                            break;

                        case MessageHeader.MessageType.leaveDungeonRequest:
                            //Leave dungeon request
                            var leaveDungeonRequest = new LeavesDungeonRequestMessage();
                            leaveDungeonRequest.DeserializeObject(ref reader);
                            playerData = serverDataHolder.players.Find(x => x.playerID == i);
                            currentRoom = serverDataHolder.rooms[playerData.roomID[0], playerData.roomID[1]];

                            if (!currentRoom.containsExit)
                            {
                                RequestDenied(leaveDungeonRequest, i);
                                return;
                            }
                            serverDataHolder.activePlayerIDs.Remove(i);
                            playerData.InDungeon = false;

                            PLayerLeftDungeonMessage leftMessage = new PLayerLeftDungeonMessage()
                            {
                                PlayerID = i
                            };
                            GlobalMessage(leftMessage);
                            NextTurn();
                            break;

                        case MessageHeader.MessageType.defendRequest:
                            //defend request
                            var defendRequest = new DefendRequestMessage();
                            defendRequest.DeserializeObject(ref reader);

                            playerData = serverDataHolder.players.Find(x => x.playerID == i);
                            currentRoom = serverDataHolder.rooms[playerData.roomID[0], playerData.roomID[1]];

                            //monster adds player to target list
                            monster = serverDataHolder.activeMonsters.Find(x => x.roomID == playerData.roomID);
                            if (!monster.targets.Contains(playerData.playerID))
                            {
                                monster.targets.Add(playerData.playerID);
                            }

                            //check is monster is here
                            if (currentRoom.containsMonster == false)
                            {
                                RequestDenied(defendRequest, i);
                                return;
                            }
                            playerData.health += 4;

                            PlayerDefendsMessage defendMessage = new PlayerDefendsMessage()
                            {
                                PlayerID = i,
                                Health = (ushort)playerData.health
                            };
                            SendMessage(defendMessage, connections[i]);
                            NextTurn();

                            break;

                        case MessageHeader.MessageType.attackRequest:
                            //attack request
                            var attackRequest = new AttackRequestMessage();
                            attackRequest.DeserializeObject(ref reader);
                            playerData = serverDataHolder.players.Find(x => x.playerID == i);
                            currentRoom = serverDataHolder.rooms[playerData.roomID[0], playerData.roomID[1]];

                            monster = serverDataHolder.activeMonsters.Find(x => x.roomID == playerData.roomID);
                            if (!monster.targets.Contains(playerData.playerID))
                            {
                                monster.targets.Add(playerData.playerID);
                            }


                            //check if room has monster
                            if (currentRoom.containsMonster == false)
                            {
                                RequestDenied(attackRequest, i);
                                return;
                            }

                            currentRoom.healthMonster -= 6;
                            if (currentRoom.healthMonster <= 0)
                            {
                                currentRoom.containsMonster = false;
                                serverDataHolder.activeMonsters.Remove(serverDataHolder.activeMonsters.Find(x => x.roomID == playerData.roomID));
                            }

                            List<int> tempIds = serverDataHolder.GetOtherPlayersInRoom(playerData);
                            tempIds.Add(i);
                            HitMonsterMessage hitMessage = new HitMonsterMessage()
                            {
                                PlayerID = i,
                                Damage = 6
                            };
                            foreach (int id in tempIds)
                            {
                                SendMessage(hitMessage, connections[serverDataHolder.players.Find(x => x.playerID == id).playerID]);
                            }
                            NextTurn();
                            break;
                        default:
                            break;
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    PlayerLeftMessage playerLeftMessage = new PlayerLeftMessage
                    {
                        PlayerLeftID = i
                    };

                    GlobalMessage(playerLeftMessage);
                    Debug.Log("Client disconnected");
                    connections[i] = default;
                }
            }
        }

        networkJobHandle = networkDriver.ScheduleUpdate();

        ProcessMessagesQueue();
    }
   

   
    private bool TryMoveRequest(MoverequestMessage message, int connectID)
    {
        PlayerSettings playerData = serverDataHolder.players.Find(x => x.playerID == connectID);
        RoomSettings currentRoom = serverDataHolder.rooms[playerData.roomID[0], playerData.roomID[1]];

        int[] nextRoom = serverDataHolder.GetNextRoomID(currentRoom, message.Direction);

        bool hasMonster = currentRoom.containsMonster && currentRoom.healthMonster > 0;
        //check direction is blocked
        if (nextRoom == null || hasMonster)
        {
            //Error failed
            return false;
        }

        //if so update dataholder of server
        serverDataHolder.players.Find(x => x.playerID == connectID).roomID = nextRoom;

        //send room info to cplayer
        RoomInfoMessage newRoomMessage = serverDataHolder.GetRoomMessage(connectID);
        SendMessage(newRoomMessage, connections[connectID]);

        //send leavmessage to players who are in the previous room
        List<int> idsInPreviousRoom = serverDataHolder.GetPlayerIDsRoom(currentRoom);
        foreach (int id in idsInPreviousRoom)
        {
            if (id != connectID)
            {
                PlayerLeftRoom(connectID, id);
            }
        }
        //Checks if there are other players in the next room
        List<int> idsInNewRoom = serverDataHolder.GetPlayerIDsRoom(serverDataHolder.rooms[nextRoom[0], nextRoom[1]]);
        foreach (int id in idsInNewRoom)
        {
            if (id != connectID)
            {
                PlayerJoinedRoom(connectID, id);
            }
        }
        return true;
    }

    private void RequestDenied(MessageHeader message, int connectID)
    {
        RequestDeniedMessage deniedMessage = new RequestDeniedMessage()
        {
            DeniedMessageID = message.ID
        };
        SendMessage(deniedMessage, connections[connectID]);
    }

    public IEnumerator MonsterAttacks(Monster monster)
    {
        networkJobHandle.Complete();

        RoomSettings currentRoom = serverDataHolder.rooms[monster.roomID[0], monster.roomID[1]];
        List<int> playerIDs = monster.targets;

        if (playerIDs.Count != 0)
        {
            yield return new WaitForSeconds(.1f);
            networkJobHandle.Complete();

            // gets random player target
            int _randomPlayerTarget = Mathf.FloorToInt(Random.Range(0, playerIDs.Count));
            PlayerSettings data = serverDataHolder.players.Find(x => x.playerID == monster.targets[_randomPlayerTarget]);

            data.health -= 5;
            if (data.health <= 0) 
            {
                PlayerDies(data, monster, _randomPlayerTarget);
            }
            else
            {
                HitByMonsterMessage hitByMessage = new HitByMonsterMessage()
                {
                    PlayerID = _randomPlayerTarget,
                    Health = (ushort)data.health
                };

                foreach (int id in playerIDs)
                {
                    SendMessage(hitByMessage, connections[id]);
                }
            }
        }
    }


    public void NextTurn()
    {
        StartCoroutine(Turning());
    }
    public IEnumerator Turning()
    {
        yield return new WaitForFixedUpdate();
        networkJobHandle.Complete();

        if (serverDataHolder.activePlayerIDs.Count == 0)
        {
            EndGame();
        }
        else
        {
            serverDataHolder.turnID += 1;
            if (serverDataHolder.turnID >= serverDataHolder.activePlayerIDs.Count)
            {
                //monsters now attack!
                foreach (Monster monster in serverDataHolder.activeMonsters)
                {
                    yield return StartCoroutine(MonsterAttacks(monster));
                    networkJobHandle.Complete();
                }
                serverDataHolder.turnID = 0;
            }
            
            if (serverDataHolder.activePlayerIDs.Count > serverDataHolder.turnID)
            {
                PlayerTurnMessage turnMessage = new PlayerTurnMessage
                {
                    PlayerID = serverDataHolder.activePlayerIDs[serverDataHolder.turnID]
                };
                GlobalMessage(turnMessage);
            } 
        }
    }

    public void EndGame()
    {
        Score[] highScorePairs = new Score[serverDataHolder.players.Count];
        for(int i = 0; i < highScorePairs.Length; i++)
        {
            highScorePairs[i].playerID = serverDataHolder.players[i].playerID;
            highScorePairs[i].score = (ushort)serverDataHolder.players[i].score;
        }
        EndGameMessage message = new EndGameMessage()
        {
            NumberOfScores = (byte)highScorePairs.Length,
            playerIDScores = highScorePairs
        };

        GlobalMessage(message);
    }

    public void StartGame()
    {
        networkJobHandle.Complete();
        StartGameMessage startMessage = new StartGameMessage
        {
            Health = 10
        };
        GlobalMessage(startMessage);
        serverDataHolder.GameSetup();

        for(int i = 0; i < connections.Length; i++)
        {
            RoomInfoMessage startRoomMessage = serverDataHolder.GetRoomMessage(i);
            SendMessage(startRoomMessage, connections[i]);
        }

        PlayerTurnMessage turnMessage = new PlayerTurnMessage
        {
            PlayerID = serverDataHolder.turnID
        };
        GlobalMessage(turnMessage);

    }

    public void NewPlayerJoined(NetworkConnection newPlayerConnection)
    {
        PlayerSettings newPlayerData = GetPlayerData(newPlayerConnection);
        NewPlayerMessage newPlayermessage = new NewPlayerMessage
        {
            PlayerID = newPlayerData.playerID,
            Colour = GameManager.Instance.ColorToUint((Color32)newPlayerData.playerColor),
            PlayerName = newPlayerData.name
        };
        GlobalMessage(newPlayermessage); 

        //send all the other player data to the new player 
        foreach (NetworkConnection conn in connections)
        {
            if (conn == newPlayerConnection) return;
            PlayerSettings otherPlayerData = GetPlayerData(conn);
            NewPlayerMessage otherPlayerMessage = new NewPlayerMessage
            {
                PlayerID = otherPlayerData.playerID,
                Colour = GameManager.Instance.ColorToUint((Color32)otherPlayerData.playerColor),
                PlayerName = otherPlayerData.name
            };
            SendMessage(otherPlayerMessage, newPlayerConnection);
        }
    }

    private PlayerSettings GetPlayerData(NetworkConnection connection)
    {
        foreach (PlayerSettings playerSettings in serverDataHolder.players)
        {
            if (playerSettings.playerID == connection.InternalId)
            {
                return playerSettings;
            }
        }
        return null;
    }

    public void PlayerDies(PlayerSettings data, Monster monster, int thisTarget)
    {
        data.health = 0;
        PlayerDiesMessage dieMessage = new PlayerDiesMessage()
        {
            PlayerID = monster.targets[thisTarget]
        };
        monster.targets.Remove(monster.targets[thisTarget]);
        serverDataHolder.activePlayerIDs.Remove(data.playerID);
        if (serverDataHolder.activePlayerIDs.Count == 0)
        {
            EndGame();
        }
        GlobalMessage(dieMessage);
    }

    public void PlayerLeftRoom(int leftId, int recieverID)
    {
        PlayerLeaveRoomMessage message = new PlayerLeaveRoomMessage()
        {
            PlayerID = leftId
        };
        SendMessage(message, connections[recieverID]);
    }

    public void PlayerJoinedRoom(int joinID, int recieverID)
    {
        PlayerEnterRoomMessage message = new PlayerEnterRoomMessage()
        {
            PlayerID = joinID
        };
        SendMessage(message, connections[recieverID]);
    }

    private void ProcessMessagesQueue()
    {
        while (messagesQueue.Count > 0)
        {
            var message = messagesQueue.Dequeue();
            ServerCallbacks[(int)message.Type].Invoke(message);
        }
    }

    private void OnDestroy()
    {
        networkDriver.Dispose();
        connections.Dispose();
    }

    private void StayAlive(int i)
    {
        SendMessage(new StayAliveMessage(), connections[i]);
    }


    public NewPlayerMessage CreateNewPlayerMessage(NetworkConnection connection)
    {
        PlayerSettings newPlayerData = GetPlayerData(connection);
        NewPlayerMessage result = new NewPlayerMessage
        {
            PlayerID = newPlayerData.playerID,
            Colour = GameManager.Instance.ColorToUint((Color32)newPlayerData.playerColor),
            PlayerName = newPlayerData.name
        };
        return result;
    }

    public void SendMessage(MessageHeader message, NetworkConnection connection)
    {
        var writer = networkDriver.BeginSend(connection);
        message.SerializeObject(ref writer);
        networkDriver.EndSend(writer);
    }

    public void GlobalMessage(MessageHeader message)
    {
        for(int i = 0; i < connections.Length; i++)
        {
            SendMessage(message, connections[i]);
        }
    }


}
