using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DivingDistance : MonoBehaviour
{
    public CharacterController player;

    void Update()
    {
        if (player.velocity.y >= 0f)
            SaveBestScore(player.transform.position.y); // Call the method to save the highscore
    }

    public void SaveBestScore(float currentScore)
    {
        float bestScore = PlayerPrefs.GetFloat("bestScore", 0f);

        if (currentScore <= bestScore)
        {
            PlayerPrefs.SetFloat("bestScore", currentScore);
            PlayerPrefs.SetString("scoreDeclaration",
                $"You dove down to a new record of {currentScore} meters!");
        }
        else
            PlayerPrefs.SetString("scoreDeclaration", $"You dove down  {currentScore}  meters." +
                $"  Your best score is {bestScore} meters.");
    }
}
