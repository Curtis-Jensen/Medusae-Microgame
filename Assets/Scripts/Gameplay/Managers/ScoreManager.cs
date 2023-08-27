using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ScoreManager : MonoBehaviour
    {
        [HideInInspector]
        public int lightScore;

        void Update()
        {
            SaveBestScore(lightScore); // Call the method to save the highscore
        }

        public void SaveBestScore(int currentScore)
        {
            float bestScore = PlayerPrefs.GetInt("bestScore", 0);

            if (currentScore >= bestScore)
            {
                PlayerPrefs.SetInt("bestScore", currentScore);
                PlayerPrefs.SetString("scoreDeclaration",
                    $"You have reached a new depth of insight: {currentScore}!  Things seem clearer now.");
            }
            else
                PlayerPrefs.SetString("scoreDeclaration", $"Although at the moment your depth of insight is only: {currentScore}, " +
                    $"you still feel and remember the time you understood at the level of: {bestScore}, and it guides you forward.");
        }
    }
}