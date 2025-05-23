﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.Game
{
    public class SpawnManager : MonoBehaviour
    {
        #region Variables
        public bool saving;
        public int waveNumber;
        public Text waveHud;
        public float endWaveDelay = 3;
        public GameObject[] possibleEnemies;

        string playerName;
        #endregion

        /* Keeps track of all the different groups that are being pooled
         * 
         * If the wave number is set to 0 that actually means that wave saving is on and it will automatically
         * chose a good wave
         * 
         * The reason the wave number is subtracted here is because EndWave() advances the wave,
         * even if it's the very first wave.  I used to have to have the starting wave always be 0 to
         * circummvent, but this is automatic.
         * 
         * And pauses the game in the beginning so the player knows how to play or that they've died
         */
        void Awake()
        {
            if (saving)
            {
                playerName = PlayerPrefs.GetString("playerName");
                waveNumber = PlayerPrefs.GetInt(playerName + "waveNumber") / 2;
            }

            StartCoroutine(EndWave());

            waveHud.text = waveNumber.ToString();
        }

        /* Advances the wave, calls spawning, and tells the appropriate scripts about the
         * increased threat
         */
        public IEnumerator EndWave()
        {
            waveNumber++;
            waveHud.text = waveNumber.ToString();
            if(saving)
                PlayerPrefs.SetInt(playerName + "waveNumber", waveNumber);

            yield return new WaitForSeconds(endWaveDelay);

            SpawnEnemies();
        }

        /* Spawns as many enemies as there are waves, and makes sure to leave active enemies alone.
         * 
         * Randomly determines what the spawn point will be based off the list of spawn points, 
         * which are determined by what the children are
         */
        void SpawnEnemies()
        {
            for (int i = 0; i < waveNumber; i++)
            {
                int randomIndex = Random.Range(0, possibleEnemies.Length);
                GameObject chosenEnemy = possibleEnemies[randomIndex];

                var nextSpawn = transform.GetChild(Random.Range(0, transform.childCount));

                Instantiate(chosenEnemy, nextSpawn.position, Quaternion.identity, gameObject.transform);
            }
        }
    }
}