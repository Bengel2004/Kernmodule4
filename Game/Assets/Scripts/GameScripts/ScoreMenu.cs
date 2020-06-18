using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class ScoreMenu : MonoBehaviour
{

    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Text scoreText;

    public void BackToMenu()
    {
        foreach(ClientBehaviour client in FindObjectsOfType<ClientBehaviour>())
        {
            Destroy(client.gameObject);
        }

        Destroy(FindObjectOfType<ServerBehaviour>());
        SceneManager.LoadScene(0);
    }

    public void ShowScore(EndGameMessage message, List<PlayerSettings> players)
    {
        List<Score> highscores = new List<Score>();
        for (int i = 0; i < message.playerIDScores.Length; i++)
        {
            highscores.Add(message.playerIDScores[i]);
        }
        highscores = highscores.OrderBy(x => x.score).Reverse().ToList();

        nameText.text = "";

        foreach (Score scores in highscores)
        {
            nameText.text += players.Find(x => x.playerID == scores.playerID).name + " " + scores.score + "\n";
            Debug.Log("TRUE");
        }

    }

}
