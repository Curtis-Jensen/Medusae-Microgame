using UnityEngine;
using UnityEngine.UI;


    public class EnemyCounter : MonoBehaviour
    {
        [Header("Enemies")]
        [Tooltip("Text component for displaying enemy objective progress")]
        public Text EnemiesText;

        EnemyManager enemyManager;

        void Awake()
        {
            enemyManager = FindObjectOfType<EnemyManager>();
            DebugUtility.HandleErrorIfNullFindObject<EnemyManager, EnemyCounter>(enemyManager, this);
        }

        void Update()
        {
            EnemiesText.text = enemyManager.NumberOfEnemiesRemaining + "/" + enemyManager.NumberOfEnemiesTotal;
        }
    }
