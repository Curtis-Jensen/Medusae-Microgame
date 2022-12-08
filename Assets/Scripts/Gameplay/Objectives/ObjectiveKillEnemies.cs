using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveKillEnemies : Objective
    {
        #region🌎 Variables
        [Tooltip("Chose whether you need to kill every enemies or only a minimum amount")]
        public bool MustKillAllEnemies = true;

        [Tooltip("If MustKillAllEnemies is false, this is the amount of enemy kills required")]
        public int KillsToCompleteObjective = 5;

        [Tooltip("Start sending notification about remaining enemies when this amount of enemies is left")]
        public int NotificationEnemiesRemainingThreshold = 3;

        int killTotal;
        #endregion

        /* 10 
         * 
         * 20 Display the objective, including by determining if all enemies need to be killed
         * or just a certain amount
         * 
         * 30 Display how many enemies are left
         */
        protected override void Start()
        {
            base.Start();

            EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled); // 10

            if (string.IsNullOrEmpty(Title)) // 20
                Title = $"Eliminate " +
                    $"{(MustKillAllEnemies ? "all the" : KillsToCompleteObjective.ToString())} enemies";

            if (string.IsNullOrEmpty(Description)) // 30
                Description = GetUpdatedCounterAmount();
        }

        /* 10 If the level is complete, it doesn't matter if an enemy dies
         */
        void OnEnemyKilled(EnemyKillEvent evt)
        {
            if (IsCompleted) // 10
                return;

            killTotal++;

            if (MustKillAllEnemies)
                KillsToCompleteObjective = evt.RemainingEnemyCount + killTotal;

            int targetRemaining = MustKillAllEnemies ? evt.RemainingEnemyCount : KillsToCompleteObjective - killTotal;

            // Complete the objective
            if (targetRemaining == 0)
                CompleteObjective(string.Empty, GetUpdatedCounterAmount(), "Objective complete : " + Title);
            else if (targetRemaining == 1) // Declare that there is only one enemy left
            {
                string notificationText = NotificationEnemiesRemainingThreshold >= targetRemaining
                    ? "One enemy left"
                    : string.Empty;
                UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
            }
            else
            {
                // Create a notification if there are not many enemies left
                string notificationText = NotificationEnemiesRemainingThreshold >= targetRemaining
                    ? targetRemaining + " enemies to kill left"
                    : string.Empty;

                UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
            }
        }

        string GetUpdatedCounterAmount()
        {
            return $"{killTotal} / {KillsToCompleteObjective}";
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
        }
    }
}