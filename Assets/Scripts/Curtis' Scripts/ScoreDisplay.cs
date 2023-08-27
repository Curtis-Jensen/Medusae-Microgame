using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.FPS.Gameplay;

public class ScoreDisplay : MonoBehaviour
{
    public ScoreManager scoreManager;
    TextMeshProUGUI scoreDisplay;

    void Start()
    {
        scoreDisplay = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (scoreManager.lightScore == 0) return;

        scoreDisplay.text = "Lights: " + scoreManager.lightScore.ToString();
    }
}
