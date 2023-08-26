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
        scoreDisplay.text = "Light level: " + scoreManager.lightScore.ToString();
    }
}
