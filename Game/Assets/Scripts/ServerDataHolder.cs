using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Monster
{
    public int[] roomID;
    public List<int> targets = new List<int>();
}
public class ServerDataHolder : MonoBehaviour
{

    public List<PlayerSettings> players;
    public List<int> activePlayerIDs;
    public List<Monster> activeMonsters;

    public RoomSettings[,] rooms;
    public int[] startIndex;
    public int size = 3;

    public int turnID = 0;
    void Start()
    {
        players = new List<PlayerSettings>();
    }

    private void CreateRoomData()
    {
        rooms = new RoomSettings[size, size];

        for (int x = 0; x < size; x++)
        {
            int containsMonsterID = Mathf.FloorToInt(UnityEngine.Random.Range(0, size));

            for (int y = 0; y < size; y++)
            {
                RoomSettings newRoom = new RoomSettings()
                {
                    directions = new Directions //set boundaries to rooms
                    {
                        North = y > 0,
                        East = x < size - 1,
                        South = y < size - 1,
                        West = x > 0
                    }
                };

                //monster placement
                if (y == containsMonsterID)
                {
                    newRoom.containsMonster = true;
                    if (UnityEngine.Random.value > .5f)
                    {
                        newRoom.treasureAmmount = 100;
                    }
                }

                //treasure placement
                if (UnityEngine.Random.value > .5f)
                {
                    newRoom.treasureAmmount = 100;
                }

                //end room with monster
                if (x == 0 && y == 0)
                {
                    newRoom.containsExit = true;
                    newRoom.treasureAmmount = 100;
                    newRoom.containsMonster = true;
                }
                rooms[x, y] = newRoom;
            }
        }


    }

    public int[] GetNextRoomID(RoomSettings currentRoom, byte dirByte)
    {
        int[] result = null;
        int[] currentIndex = Tools.FindIndex(rooms, currentRoom); //needs testing!
        int xPos = currentIndex[0];
        int yPos = currentIndex[1];
        DirectionEnum directionEnum = (DirectionEnum)dirByte;
        switch (directionEnum)
        {
            case DirectionEnum.North:
                if (currentRoom.directions.North && yPos >  0)
                {
                    result = new int[]{ xPos, yPos - 1};
                }
                break;
            case DirectionEnum.East: 
                if (currentRoom.directions.East && xPos < size - 1)
                {
                    result = new int[] { xPos + 1, yPos };
                }
                break;
            case DirectionEnum.South:
                if (currentRoom.directions.South && yPos < size - 1)
                { 
                    result = new int[] { xPos, yPos + 1 };
                }
                break;
            case DirectionEnum.West:
                if (currentRoom.directions.West && xPos > 0)
                {
                    result = new int[] { xPos - 1, yPos };
                }
                break;
            default:
                break;
        }
        if (result != null)
        {
        }

        return result;
    }


    public void GameSetup()
    {
        CreateRoomData();

        //setup start room
        startIndex = new int[] { size -1, size - 1};
        rooms[startIndex[0], startIndex[1]].containsMonster = false;
        rooms[startIndex[0], startIndex[1]].containsExit = true;
        rooms[startIndex[0], startIndex[1]].treasureAmmount = 100;

        activePlayerIDs = new List<int>();
        activeMonsters = new List<Monster>();
        foreach(PlayerSettings player in players)
        {
            player.roomID = startIndex;
            player.health = 10; 
            activePlayerIDs.Add(player.playerID);
        }
    } 

    public RoomInfoMessage GetRoomMessage(int playerID)
    {
        PlayerSettings data = players.Find(x => x.playerID == playerID); 

        int[] roomIndex = data.roomID;
        RoomSettings room = rooms[roomIndex[0], roomIndex[1]];

        List<int> otherPlayers = GetOtherPlayersInRoom(data);

        // contains monster
        if (room.containsMonster)
        {
            if (!activeMonsters.Contains(activeMonsters.Find(x => x.roomID == data.roomID)))
            {
                activeMonsters.Add(new Monster() {
                    roomID = data.roomID
                });
            }
        }

        return new RoomInfoMessage()
        {
            MoveDirections = room.GetDirsByte(),
            TreasureRoom = (ushort)room.treasureAmmount,
            ContainsMonster = (byte)(room.containsMonster ? 1 : 0),
            ContainsExit = (byte)(room.containsExit ? 1 : 0),
            NumberOfOtherPlayers = (byte)otherPlayers.Count,
            OtherPlayerIDs = otherPlayers
        }; 

    }

    public List<int> GetOtherPlayersInRoom(PlayerSettings data)
    {
        List<int> result = new List<int>();

        for (int i = 0; i < players.Count; i++)
        {
            // research this
            if (players[i].roomID[0] == data.roomID[0] && players[i].roomID[1] == data.roomID[1] && players[i].playerID != data.playerID && players[i].InDungeon && players[i].health > 0)
            {
                result.Add(players[i].playerID);
            } 
        }
        return result;
    } 

    public List<int> GetPlayerIDsRoom(RoomSettings data)
    {
        List<int> result = new List<int>();
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].roomID[0] == Tools.FindIndex(rooms, data)[0] && players[i].roomID[1] == Tools.FindIndex(rooms, data)[1])
            {
                result.Add(players[i].playerID);
            }
        }
        return result;
    }

}
