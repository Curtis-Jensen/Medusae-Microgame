using Unity.FPS.AI;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
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
}