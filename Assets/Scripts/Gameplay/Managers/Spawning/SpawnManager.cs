using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.FPS.UI;

namespace Unity.FPS.UI
{
    public class SpawnManager : MonoBehaviour
    {
        #region Variables
        [Tooltip("We need to cap total enemies for performance.")]
        public int enemyCap;
        public int waveNumber;
        [Tooltip("The amount of seconds it takes for the next wave to start automatically")]
        public float waveTimeLimit;
        [Range(0, 1)]
        public float medusaChance;
        public GameObject medusaPrefab;
        public GameObject hoverBotPrefab;
        public Transform sunTransform;
        public Text waveHud;

        Transform spawnerList;
        bool nextWave = false;
        float waveTimer;
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
            spawnerList = GameObject.Find("Enemy Spawn Points").transform;
            playerName = PlayerPrefs.GetString("playerName");
            if (waveNumber == 0)
                waveNumber = PlayerPrefs.GetInt(playerName + "waveNumber") / 2;

            EndWave();

            waveHud.text = waveNumber.ToString();
        }

        /* Advances the wave, calls spawning, and tells the appropriate scripts about the
         * increased threat
         */
        public void EndWave()
        {
            waveNumber++;

            PlayerPrefs.SetInt(playerName + "waveNumber", waveNumber);
            SpawnEnemies();

            waveHud.text = waveNumber.ToString();
        }

        /* Spawns as many enemies as there are waves, and makes sure to leave active enemies alone.
         */
        void SpawnEnemies()
        {
            GameObject chosenEnemy;

            for (int i = 0; i < waveNumber; i++)
            {
                if (Random.value > medusaChance)
                    chosenEnemy = medusaPrefab;
                else
                    chosenEnemy = hoverBotPrefab;

                var nextSpawn = spawnerList.GetChild(Random.Range(0, spawnerList.childCount)).gameObject;

                Instantiate(chosenEnemy, nextSpawn.transform.position, Quaternion.identity, gameObject.transform);
            }
        }
    }
}