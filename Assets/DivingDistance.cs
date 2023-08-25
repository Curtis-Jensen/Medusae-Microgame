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
                $"You have reached a new level of depth of isight: {currentScore}!  Things seem clearer now.");
        }
        else
            PlayerPrefs.SetString("scoreDeclaration", $"Although at the moment your depth of insight is only: {currentScore}, " +
                $"you still feel and remember the time you understood at the level of: {bestScore}, and it guides you forward.");
    }
}
