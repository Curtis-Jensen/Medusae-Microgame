using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreDisplay : MonoBehaviour
{
    TextMeshProUGUI highScoreDisplay;
    [TextArea(1, 10)]
    public string highScoreMessage;    
    [TextArea(1, 10)]
    public string regularMessage;

    void Start()
    {
        highScoreDisplay = GetComponent<TextMeshProUGUI>();

        var recentScore = PlayerPrefs.GetInt("recentScore");
        var highScore = PlayerPrefs.GetInt("highScore");

        if (recentScore > highScore)
        {
            PlayerPrefs.SetInt("highScore", recentScore);
            highScoreDisplay.text = string.Format(highScoreMessage, recentScore); ;
        }
        else
            highScoreDisplay.text = string.Format(regularMessage, recentScore, highScore); ;
    }
}
