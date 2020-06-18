using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DataBase : MonoBehaviour
{
    [HideInInspector] public string baseUrl = "https://studenthome.hku.nl/~niels.poelder/Kernmodule4/";
    public string dataResult;

    public int playerID;
    public int serverID;
    public int Score;

    public static DataBase Instance = null;

    public GameObject loginMenu;
    public Text email;
    public Text password;

    public UserData data;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        ServerLogin();
        if (data.id != "" && data.id != "0" && data.id != null)
        {
            loginMenu.SetActive(false);
        }
    }

    public void ServerLogin()
    {
        StartCoroutine(GetHttp(baseUrl + "serverlogin.php?id=1&password=Kipje"));
        serverID = 1;
    }

    public void userLogin()
    {
        StartCoroutine(Login(email.text, password.text));
    }

    public IEnumerator Login(string _username, string _password)
    {
        yield return StartCoroutine(GetHttp(baseUrl + "login.php?email=" + _username + "&password=" + _password ));
        // fixes json bug
        var jsonStart = dataResult.IndexOf('{');
        var jsonEnd = dataResult.LastIndexOf('}');
        if(jsonStart != -1 && jsonEnd != -1)
        {
            var json = dataResult.Substring(jsonStart, jsonEnd - jsonStart + 1);
            data = JsonUtility.FromJson<UserData>(json);
        }

        if(data.id != "" && data.id != "0" && data.id != null)
        {
            loginMenu.SetActive(false);
            playerID = int.Parse(data.id);
        }

    }

    public void InsertScore(int _playerID, int _serverId, int _score)
    {
        StartCoroutine(GetHttp(baseUrl + "insertscore.php?uid=" + _playerID + "&sid=" + _serverId + "&score=" + _score));
    }


    async void GetHttpAsync()
    {
        using (var _client = new HttpClient())
        {
            var _result = await _client.GetAsync("url");
            if (_result.IsSuccessStatusCode)
            {
                Debug.Log(await _result.Content.ReadAsStringAsync());
            }
        }
    }

    IEnumerator GetHttp(string _url)
    {
        var _request = UnityWebRequest.Get(_url);
        {
            yield return _request.SendWebRequest();

            if (_request.isDone && !_request.isHttpError)
            {
                dataResult = _request.downloadHandler.text;
            }
        }
    }
}

[System.Serializable]
public class UserData
{
    public string id;
    public string first_name;
    public string last_name;
    public string email;
    public string password;
    public string birthday;
}
