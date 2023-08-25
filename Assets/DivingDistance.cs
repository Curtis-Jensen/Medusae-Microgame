using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DivingDistance : MonoBehaviour
{
    public CharacterController player;

    TextMeshProUGUI distanceText;

    void Start()
    {
        distanceText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (player.velocity.y >= 0f)
        {
            var playerPosition = -(int)player.transform.position.y;
            SaveBestScore(playerPosition); // Call the method to save the highscore
            distanceText.text = playerPosition.ToString();
        }
    }

    public void SaveBestScore(int currentScore)
    {
        float bestScore = PlayerPrefs.GetInt("bestScore", 0);

        if (currentScore >= bestScore)
        {
            PlayerPrefs.SetInt("bestScore", currentScore);
            PlayerPrefs.SetString("scoreDeclaration",
                $"You have reached a new depth of understanding: {currentScore}.");
        }
        else
            PlayerPrefs.SetString("scoreDeclaration", $"Thinking through it, it doesn’t make as much sense: {currentScore}, " +
                $"but you still feel and remember the time you understood more: {bestScore}.");
    }
}
