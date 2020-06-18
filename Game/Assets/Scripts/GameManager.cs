using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public PlayerSettings pSettings;
    public List<PlayerSettings> otherPlayers;
    public LobbyProtocol lobby;
    public Game game;
    public ScoreMenu score;
    public ClientBehaviour client;
    public static GameManager Instance = null;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        pSettings = new PlayerSettings();
        otherPlayers = new List<PlayerSettings>();
    }

    public void StartGame()
    {
        lobby.gameObject.SetActive(false);
        game = Instantiate(Resources.Load<GameObject>("LevelPrefab")).GetComponent<Game>();
        game.dataHolder = this;
    }

    public void ShowEndScreen(EndGameMessage message)
    {
        game.gameObject.SetActive(false);
        score = Instantiate(Resources.Load<GameObject>("Score")).GetComponent<ScoreMenu>();
        score.ShowScore(message, otherPlayers);
        DataBase.Instance.InsertScore(DataBase.Instance.playerID, 1, (int)pSettings.score);
    }

    public Color UIntToColor(uint color)
    {
        float r = (byte)(color >> 24);
        float g = (byte)(color >> 16);
        float b = (byte)(color >> 8);
        float a = (byte)(color >> 0);

        return new Color(r / 255, g / 255, b / 255, a / 255);
    }

    public uint ColorToUint(Color32 colour)
    {
        return ((uint)colour.r << 24) | ((uint)colour.g << 16) | ((uint)colour.b << 8) | colour.a;
    }

}
[System.Serializable]
public class PlayerSettings
{
    public string name;
    public int playerID;
    public int health = 0;
    public int[] roomID;
    public int score = 0;
    public bool InDungeon = true;
    public Color playerColor;
}