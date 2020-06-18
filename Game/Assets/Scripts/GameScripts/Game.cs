using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//game from the client side
public class Game : MonoBehaviour
{
    public GameManager dataHolder;
    public RoomSettings roomSettings = new RoomSettings();
    public int currentTurnID;
    public Button monsterAttack;
    public Button monsterDefend;
    public TextMesh monsterText;
    public Button exit;
    public Button treasure;
    public Button[] doors;
    public TextMesh[] otherPlayers;
    public TextMesh myPlayerObj;

    public GameObject buttonParent;
    public Text turnText;
    public Text hp;
    public Text score;
    public Text gameOverText;
    public List<PlayerSettings> otherPlayersInRoom = new List<PlayerSettings>();

    private bool inDungeon = true;


    public void UpdateInfo(RoomInfoMessage message)
    {

        roomSettings.treasureAmmount = message.TreasureRoom;
        roomSettings.containsMonster = message.ContainsMonster == 1;
        if (roomSettings.containsMonster)
        {
            roomSettings.healthMonster = 10;
        }
        roomSettings.containsExit = message.ContainsExit == 1;
        roomSettings.numberOfOtherPlayers = message.NumberOfOtherPlayers;
        roomSettings.otherPlayersIDs = message.OtherPlayerIDs;
        roomSettings.directions = roomSettings.ReadDirectionByte(message.MoveDirections);
        UpdateRoom();

    }

    public void UpdateRoom()
    {
        if (otherPlayersInRoom == null) { otherPlayersInRoom = new List<PlayerSettings>(); }
        otherPlayersInRoom.Clear();

        foreach (int id in roomSettings.otherPlayersIDs)
        {
            PlayerJoinedRoom(dataHolder.otherPlayers.Find(x => x.playerID == id));
        }
        UpdatePlayersUI();
    }

    public void UpdateHPText()
    {
        hp.text = "Health: " + dataHolder.pSettings.health;
        monsterText.text = "Monster Health: " + roomSettings.healthMonster;
    }

    public void UpdateUI(bool myTurn, int playerID)
    {
        buttonParent.SetActive(myTurn);

        for (int i = 0; i < doors.Length; i++)
        {
            switch (i)
            {
                case 0:
                    doors[i].gameObject.SetActive(myTurn && !roomSettings.containsMonster && roomSettings.directions.North);
                    break;
                case 1:
                    doors[i].gameObject.SetActive(myTurn && !roomSettings.containsMonster && roomSettings.directions.East);
                    break;
                case 2:
                    doors[i].gameObject.SetActive(myTurn && !roomSettings.containsMonster && roomSettings.directions.South);
                    break;
                case 3:
                    doors[i].gameObject.SetActive(myTurn && !roomSettings.containsMonster && roomSettings.directions.West);
                    break;

            }
        }
        treasure.gameObject.SetActive(myTurn && roomSettings.treasureAmmount > 0 && !roomSettings.containsMonster);
        exit.gameObject.SetActive(myTurn && !roomSettings.containsMonster);
        monsterAttack.gameObject.SetActive(myTurn && roomSettings.containsMonster);
        monsterDefend.gameObject.SetActive(myTurn && roomSettings.containsMonster);
        monsterText.gameObject.SetActive(myTurn && roomSettings.containsMonster);

        hp.text = "Health: " + dataHolder.pSettings.health;
        monsterText.text = "Monster Health: " + roomSettings.healthMonster;
        score.text = "Score: " + dataHolder.pSettings.score;

        string name = otherPlayersInRoom.Find(x => x.playerID == playerID)?.name;
        if (myTurn)
        {
            turnText.text = "Your turn!";
        }
        else
        {
            turnText.text = playerID + " " + name + "'s turn...";
        }

    }
    public void PlayerJoinedRoom(PlayerSettings data)
    {
        otherPlayersInRoom.Add(data);
    }
    public void PlayerLeftRoom(PlayerSettings data)
    {
        otherPlayersInRoom.Remove(data);
    }

    public void ILeftDungeon()
    {
        inDungeon = false;
    }

    public void GameOver()
    {
        dataHolder.pSettings.health = 0;
        UpdatePlayersUI();
        gameOverText.gameObject.SetActive(true);
    }

    public void UpdatePlayersUI()
    {
        myPlayerObj.gameObject.SetActive(inDungeon && dataHolder.pSettings.health > 0);
        myPlayerObj.text = dataHolder.pSettings.name;

        foreach(TextMesh player in otherPlayers)
        {
            player.gameObject.SetActive(false);
        }

        for (int i = 0; i < otherPlayersInRoom.Count; i++)
        {
            if (otherPlayersInRoom[i] != null)
            {
                otherPlayers[i].gameObject.SetActive(true);
                otherPlayers[i].text = otherPlayersInRoom[i].name;
            }
        }
    }

    public void UpdateRoom(RoomInfoMessage message)
    {
        UpdateInfo(message);
    }

    public void MoveWest()
    {
        dataHolder.client.MoveRequest(DirectionEnum.West);
    }

    public void MoveNorth()
    {
        dataHolder.client.MoveRequest(DirectionEnum.North);

    }

    public void MoveSouth()
    {
        dataHolder.client.MoveRequest(DirectionEnum.South);
    }

    public void MoveEast()
    {
        dataHolder.client.MoveRequest(DirectionEnum.East);
    }

    public void ExitDungeon()
    {
        dataHolder.client.SendMessage(new LeavesDungeonRequestMessage());
    }

    public void ClaimTreasure()
    {
        dataHolder.client.SendMessage(new ClaimTreasureRequestMessage());
        treasure.gameObject.SetActive(false);
    }

    public void RequestAttackMonster()
    {
        dataHolder.client.SendMessage(new AttackRequestMessage());
    }
    public void RequestDefendAgainstMonster()
    {
        dataHolder.client.SendMessage(new DefendRequestMessage());
    }

    public void PlayerTurn(int _id)
    {
        currentTurnID = _id;
        if (currentTurnID == dataHolder.pSettings.playerID)
        {
            UpdateUI(true, currentTurnID);
        }
        else
        {
            UpdateUI(false, currentTurnID);
        }
    }
}









































