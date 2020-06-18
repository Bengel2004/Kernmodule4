using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyProtocol : MonoBehaviour
{

    [SerializeField]
    private GameObject MainMenu;
    [SerializeField]
    private GameObject readyButton;

    [SerializeField]
    private Text players;

    [SerializeField]
    private GameObject startButton;

    public bool isHost = false;

    private ClientBehaviour client;

    private ServerBehaviour serverBehaviour;

    public void Start()
    {
        startButton.SetActive(false);
        readyButton.SetActive(false);
        players.transform.parent.gameObject.SetActive(false);
    }

    public void CreateClient(bool _isHost = false)
    {
        isHost = _isHost;

        GameObject _client = new GameObject("client");
        if (isHost)
        {
            serverBehaviour = _client.AddComponent<ServerBehaviour>();
        }

        _client.AddComponent<GameManager>().lobby = this;
        client = _client.AddComponent<ClientBehaviour>();
        DontDestroyOnLoad(_client);

        MainMenu.SetActive(false);
        readyButton.SetActive(true);
    }

    public void SetName()
    {
        readyButton.SetActive(false);

        var setNameMessage = new SetNameMessage
        {
            Name = "" + DataBase.Instance.data.first_name
        };
        client.SendMessage(setNameMessage);
    }

    public void StartGame()
    {
        gameObject.SetActive(false);
        players.transform.parent.gameObject.SetActive(false);
    }

    public void StartButtonClicked()
    {
        if (isHost) 
        {
            serverBehaviour.StartGame();
        }
    }

    public void UpdateLobby(PlayerSettings[] _pDatas)
    {
        startButton.SetActive(isHost && _pDatas.Length != 0);
        players.transform.parent.gameObject.SetActive(true);
        string _content = "";

        foreach(PlayerSettings data in _pDatas)
        {
            _content +=  data.name + ", color: " + data.playerColor.r + data.playerColor.g + data.playerColor.b + "\n";
        }
        players.text = _content;
    }
}
