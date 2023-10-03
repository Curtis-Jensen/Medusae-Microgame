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

    void Start()
    {
        highScoreDisplay = GetComponent<TextMeshProUGUI>();

        var recentScore = PlayerPrefs.GetInt("recentScore");
        var highScore = PlayerPrefs.GetInt("highScore");
        highScoreDisplay.text = highScoreMessage;
    }
}
