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
                $"You have reached a new depth of understanding: {currentScore}.");
        }
        else
            PlayerPrefs.SetString("scoreDeclaration", $"Thinking through it, it doesn’t make as much sense: {currentScore}, " +
                $"but you still feel and remember the time you understood: {bestScore}.");
    }
}
