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
        public int enemyCap;//we need to cap it for performance.
        public int waveNumber;
        public int pageAppear = 19;
        public float waveTimeLimit;//The amount of seconds it takes for the next wave to start automatically
        [Range(0, 1)]
        public float medusaChance;
        public float damageMultiplier = 5f; //The amount of damage a medusa will do.
        public GameObject medusaPrefab;
        public GameObject runnerPrefab;
        public GameObject endingObject;
        public Transform sunTransform;
        public Text waveText;

        Transform spawnList;
        GameObject[] enemyGroup;
        GameObject nextSpawn;
        GameObject chosenEnemy;
        bool nextWave = false;
        float waveTimer;
        int activeEnemies;
        int totalEnemies;
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
            enemyGroup = new GameObject[enemyCap];
            CreateEnemyPool();
            spawnList = GameObject.Find("Enemy Spawn Points").transform;
            playerName = PlayerPrefs.GetString("playerName");
            if (waveNumber == 0)
            {
                waveNumber = PlayerPrefs.GetInt(playerName + "waveNumber") / 2;
                if (waveNumber == 0) waveNumber = 1;
            }

            waveText.text = "DAY:" + waveNumber;
        }

        /* For Each enemy in the enemy group:
         * Decide which enemy type will be used by doing a random percent, if it's big enough then
         * a more special option will be used.
         */
        void CreateEnemyPool()
        {
            for (int i = 0; i < enemyCap; i++)
            {
                if (Random.value > medusaChance)//Could probably have a public array of prefabs to condense this a bit
                    chosenEnemy = medusaPrefab;
                else
                    chosenEnemy = runnerPrefab;

                GameObject enemy =
                    Instantiate(chosenEnemy, new Vector3(0, 0, 0), Quaternion.identity, gameObject.transform);
                enemy.SetActive(false);
                enemyGroup[i] = enemy;
            }
        }

        void Update()
        {
            ChangeTime();
        }

        /* Advances time and increments the day counter if daytime is survived. 
         * 
         * The reason nextWave exists is because the waves advance and the sun set at two different times,
         * so two different if statements need to be called
         */
        void ChangeTime()
        {
            waveTimer += Time.deltaTime;
            if (waveTimer > waveTimeLimit / 2 && !nextWave)
            {
                EndWave();
                nextWave = true;
            }
            else if (waveTimer > waveTimeLimit)
            {
                waveTimer = 0;
                nextWave = false;
            }

            sunTransform.rotation =
                Quaternion.Euler(new Vector3(waveTimer / waveTimeLimit * 360, 0, 0));
        }

        /*Advances the wave, calls spawning, and tells the appropriate scripts about the
         * increased threat
         */
        public void EndWave()
        {
            waveNumber++;

            if (waveNumber > pageAppear) endingObject.SetActive(true);
            PlayerPrefs.SetInt(playerName + "waveNumber", waveNumber);
            SpawnEnemies();

            waveText.text = waveNumber.ToString();
        }

        /* randomly increases or decreases the number of enemies based off how many waves have gone by.
         * 
         * Spawns as many enemies as there are waves, and makes sure to leave active enemies alone.
         */
        void SpawnEnemies()
        {
            for (int i = 0; i < waveNumber; i++)
            {
                while (enemyGroup[i + activeEnemies].activeInHierarchy) activeEnemies++;

                totalEnemies = i + activeEnemies;
                nextSpawn = spawnList.GetChild(Random.Range(0, spawnList.childCount)).gameObject;

                enemyGroup[totalEnemies].transform.position = nextSpawn.transform.position;
                enemyGroup[totalEnemies].SetActive(true);
            }
        }
    }
}