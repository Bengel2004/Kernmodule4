using UnityEngine;
using System.Collections;
using Unity.Networking.Transport;
using System.IO;
using Unity.Jobs;

public class ClientBehaviour : MonoBehaviour
{
    private NetworkDriver networkDriver;
    private NetworkConnection connection;

    private JobHandle networkJobHandle;

    private float timePassed = 0;
    private float aliveDuration = 10;

    void Start()
    {
        GameManager.Instance.client = this;

        networkDriver = NetworkDriver.Create();
        connection = default;

        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 9000;
        connection = networkDriver.Connect(endpoint);
    }
    void Update()
    {
        networkJobHandle.Complete();

        if (!connection.IsCreated)
        {
            return;
        }

        DataStreamReader reader;
        NetworkEvent.Type cmd;
        while ((cmd = connection.PopEvent(networkDriver, out reader)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("Connected to server");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                var messageType = (MessageHeader.MessageType)reader.ReadUShort();
                switch (messageType)
                {
                    case MessageHeader.MessageType.none:
                        break;
                    case MessageHeader.MessageType.newPlayer:
                        var newplayermessage = new NewPlayerMessage();
                        newplayermessage.DeserializeObject(ref reader);
                        PlayerSettings data = new PlayerSettings
                        {
                            playerID = newplayermessage.PlayerID,
                            playerColor = GameManager.Instance.UIntToColor(newplayermessage.Colour),
                            name = newplayermessage.PlayerName
                        };

                        if (data.playerID == GameManager.Instance.pSettings.playerID)
                        {
                            GameManager.Instance.pSettings.name = data.name;
                        }
                        GameManager.Instance.otherPlayers.Add(data);
                        GameManager.Instance.lobby.UpdateLobby(GameManager.Instance.otherPlayers.ToArray());
                        break;
                    case MessageHeader.MessageType.welcome:
                        var welcomeMessage = new WelcomeMessage();
                        welcomeMessage.DeserializeObject(ref reader);
                        GameManager.Instance.pSettings.playerID = welcomeMessage.PlayerID;
                        GameManager.Instance.pSettings.playerColor = GameManager.Instance.UIntToColor(welcomeMessage.Colour);
                        break;
                    case MessageHeader.MessageType.requestDenied:
                        break;
                    case MessageHeader.MessageType.playerLeft:
                        var playerLeftMessage = new PlayerLeftMessage();
                        playerLeftMessage.DeserializeObject(ref reader);

                        PlayerSettings removedData = GameManager.Instance.otherPlayers.Find(x => x.playerID == playerLeftMessage.PlayerLeftID);
                        GameManager.Instance.otherPlayers.Remove(removedData);
                        GameManager.Instance.lobby.UpdateLobby(GameManager.Instance.otherPlayers.ToArray());
                        break;
                    case MessageHeader.MessageType.startGame:
                        var startGameMessage = new StartGameMessage();
                        startGameMessage.DeserializeObject(ref reader);
                        GameManager.Instance.pSettings.health = startGameMessage.Health;
                        GameManager.Instance.StartGame();
                        break;
                    case MessageHeader.MessageType.roomInfo:
                        var roomInfoMessage = new RoomInfoMessage();
                        roomInfoMessage.DeserializeObject(ref reader);
                        GameManager.Instance.game.UpdateRoom(roomInfoMessage);
                        break;
                    case MessageHeader.MessageType.playerEnterRoom:
                        var entermessage = new PlayerEnterRoomMessage();
                        entermessage.DeserializeObject(ref reader);
                        GameManager.Instance.game.PlayerJoinedRoom(GameManager.Instance.otherPlayers.Find(x => x.playerID == entermessage.PlayerID));
                        GameManager.Instance.game.UpdatePlayersUI();
                        break;
                    case MessageHeader.MessageType.playerLeaveRoom:
                        var leavemessage = new PlayerLeaveRoomMessage();
                        leavemessage.DeserializeObject(ref reader);
                        GameManager.Instance.game.PlayerLeftRoom(GameManager.Instance.otherPlayers.Find(x => x.playerID == leavemessage.PlayerID));
                        GameManager.Instance.game.UpdatePlayersUI();
                        break;
                    case MessageHeader.MessageType.playerTurn:
                        var turnmessage = new PlayerTurnMessage();
                        turnmessage.DeserializeObject(ref reader);
                        GameManager.Instance.game.PlayerTurn(turnmessage.PlayerID);
                        break;
                    case MessageHeader.MessageType.obtainTreasure:
                        var treasuremessage = new ObtainTreasureMessage();
                        treasuremessage.DeserializeObject(ref reader);

                        GameManager.Instance.pSettings.score += treasuremessage.Amount;

                        GameManager.Instance.game.treasure.gameObject.SetActive(false);
                        break;
                    case MessageHeader.MessageType.playerLeftDungeon:
                        var playerleftmessage = new PLayerLeftDungeonMessage();
                        playerleftmessage.DeserializeObject(ref reader);
                        if (playerleftmessage.PlayerID == GameManager.Instance.pSettings.playerID)
                        {
                            GameManager.Instance.game.ILeftDungeon();
                        }
                        else
                        {
                            PlayerSettings leftPlayer = GameManager.Instance.game.otherPlayersInRoom.Find(x => x.playerID == playerleftmessage.PlayerID);
                            if (leftPlayer != null)
                            {
                                GameManager.Instance.game.PlayerLeftRoom(leftPlayer);
                            }
                        }
                        GameManager.Instance.game.UpdatePlayersUI();
                        break;
                    case MessageHeader.MessageType.endGame:
                        var endgamemessage = new EndGameMessage();
                        endgamemessage.DeserializeObject(ref reader);

                        Score[] endScores = endgamemessage.playerIDScores;

                        GameManager.Instance.ShowEndScreen(endgamemessage);
                        break;
                        case MessageHeader.MessageType.hitMonster:
                        var hitmonstermessage = new HitMonsterMessage();
                        hitmonstermessage.DeserializeObject(ref reader);
                        GameManager.Instance.game.roomSettings.healthMonster -= hitmonstermessage.Damage;
                        if (GameManager.Instance.game.roomSettings.healthMonster <= 0)
                        {
                            GameManager.Instance.game.roomSettings.containsMonster = false;
                        }
                        GameManager.Instance.game.UpdateHPText();
                        GameManager.Instance.game.UpdateRoom();
                        break;
                    case MessageHeader.MessageType.hitByMonster:
                        var hitbymonster = new HitByMonsterMessage();
                        hitbymonster.DeserializeObject(ref reader);

                        if (hitbymonster.PlayerID == GameManager.Instance.pSettings.playerID)
                        {
                            GameManager.Instance.pSettings.health = hitbymonster.Health;
                            GameManager.Instance.game.UpdateHPText();
                        }
                        break;
                    case MessageHeader.MessageType.playerDies:
                        var playerdiesmessage = new PlayerDiesMessage();
                        playerdiesmessage.DeserializeObject(ref reader);

                        if (playerdiesmessage.PlayerID == GameManager.Instance.pSettings.playerID)
                        {
                            GameManager.Instance.game.GameOver();
                        }
                        else
                        {
                            GameManager.Instance.game.PlayerLeftRoom(GameManager.Instance.otherPlayers.Find(x => x.playerID == playerdiesmessage.PlayerID));
                        }
                        GameManager.Instance.game.UpdatePlayersUI();
                        break;
                    case MessageHeader.MessageType.playerDefends:
                        var defendmessage = new PlayerDefendsMessage();
                        defendmessage.DeserializeObject(ref reader);

                        if (defendmessage.PlayerID == GameManager.Instance.pSettings.playerID)
                        {
                            GameManager.Instance.pSettings.health = defendmessage.Health;
                            GameManager.Instance.game.UpdateHPText();
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Disconnected");
                connection = default;
            }
        }
        CheckAliveSend();

        networkJobHandle = networkDriver.ScheduleUpdate();
    }
    public void MoveRequest(DirectionEnum Enum)
    {
        var moveRequest = new MoverequestMessage();
        moveRequest.Direction = (byte)Enum;
        SendMessage(moveRequest);
    }
    public void HitMonster(ref DataStreamReader reader)
    {
        var message = new HitMonsterMessage();
        message.DeserializeObject(ref reader);
        GameManager.Instance.game.roomSettings.healthMonster -= message.Damage;
        if (GameManager.Instance.game.roomSettings.healthMonster <= 0)
        {
            GameManager.Instance.game.roomSettings.containsMonster = false;
        }
        GameManager.Instance.game.UpdateHPText();
        GameManager.Instance.game.UpdateRoom();
    }
    public void HitByMonster(ref DataStreamReader reader)
    {
        var message = new HitByMonsterMessage();
        message.DeserializeObject(ref reader);
        if (message.PlayerID == GameManager.Instance.pSettings.playerID)
        {
            GameManager.Instance.pSettings.health = message.Health;
            GameManager.Instance.game.UpdateHPText();
        }
    }
    public void PlayerDies(ref DataStreamReader reader)
    {
        var message = new PlayerDiesMessage();
        message.DeserializeObject(ref reader);

        if (message.PlayerID == GameManager.Instance.pSettings.playerID)
        {
            GameManager.Instance.game.GameOver();
        }
        else
        {
            GameManager.Instance.game.PlayerLeftRoom(GameManager.Instance.otherPlayers.Find(x => x.playerID == message.PlayerID));
        }
        GameManager.Instance.game.UpdatePlayersUI();
    }
    public void DefendsAgainstMonster(ref DataStreamReader reader)
    {
        var message = new PlayerDefendsMessage();
        message.DeserializeObject(ref reader);
        if (message.PlayerID == GameManager.Instance.pSettings.playerID)
        {
            GameManager.Instance.pSettings.health = message.Health;
            GameManager.Instance.game.UpdateHPText();
        }
    }
    public void PlayerLeftDungeon(ref DataStreamReader reader)
    {
        var message = new PLayerLeftDungeonMessage();
        message.DeserializeObject(ref reader);
        if (message.PlayerID == GameManager.Instance.pSettings.playerID)
        {
            GameManager.Instance.game.ILeftDungeon();
        }
        else
        {
            PlayerSettings leftPlayer = GameManager.Instance.game.otherPlayersInRoom.Find(x => x.playerID == message.PlayerID);
            if (leftPlayer != null)
            {
                GameManager.Instance.game.PlayerLeftRoom(leftPlayer);
            }
        }
        GameManager.Instance.game.UpdatePlayersUI();
    }
    public void EndGame(ref DataStreamReader reader)
    {
        var message = new EndGameMessage();
        message.DeserializeObject(ref reader);
        Score[] endScores = message.playerIDScores;
        GameManager.Instance.ShowEndScreen(message);

    }
    private void CheckAliveSend()
    {
        timePassed += Time.deltaTime;
        if (timePassed > aliveDuration)
        {
            timePassed = 0;
            StayAlive();
        }
    }

    private void OnDestroy()
    {
        networkJobHandle.Complete();
        networkDriver.Dispose();
    }

    private void StayAlive()
    {
        SendMessage(new StayAliveMessage());
    }

    public void SendMessage(MessageHeader message)
    {
        networkJobHandle.Complete();
        var _writer = networkDriver.BeginSend(connection);
        message.SerializeObject(ref _writer);
        networkDriver.EndSend(_writer);
    }
}