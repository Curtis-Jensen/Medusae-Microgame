using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreDisplay : MonoBehaviour
{
    TextMeshProUGUI highScoreDisplay;
    [TextArea(1, 10)]
    public string[] highScoreMessages;    
    [TextArea(1, 10)]
    public string[] regularMessages;

    void Start()
    {
        // Instantiates objects and gets high scores
        highScoreDisplay = GetComponent<TextMeshProUGUI>();

        var recentScore = PlayerPrefs.GetInt("recentScore");
        var highScore = PlayerPrefs.GetInt("highScore");

        // Decides if it's a high score or a regular score and displays the message
        if (recentScore > highScore)
        {
            PlayerPrefs.SetInt("highScore", recentScore);
            int random = Random.Range(0, highScoreMessages.Length);
            highScoreDisplay.text = string.Format(highScoreMessages[random], recentScore); ;
        }
        else
        {
            int random = Random.Range(0, regularMessages.Length);
            highScoreDisplay.text = string.Format(regularMessages[random], recentScore, highScore);
        }
    }
}
