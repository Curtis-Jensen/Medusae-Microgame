using System.Collections;
using Unity.FPS.Game;
using UnityEngine;

public class ObjectiveSurvive : Objective
{
    [Tooltip("Chose whether you need to kill every enemies or only a minimum amount")]
    public bool MustKillAllEnemies = true;

    [Tooltip("If MustKillAllEnemies is false, this is the amount of enemy kills required")]
    public int KillsToCompleteObjective = 5;

    [Tooltip("Start sending notification about remaining enemies when this amount of enemies is left")]
    public int NotificationEnemiesRemainingThreshold = 3;

    [Header("Spawning")]
    public SpawnManager spawnManager;
    public float endWaveDelay = 3;

    int killTotal;

    protected override void Start()
    {
        base.Start();

        EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);

        // set a title and description specific for this type of objective, if it doesn't have one
        if (string.IsNullOrEmpty(Title))
            Title = "Eliminate " + (MustKillAllEnemies ? "all the" : KillsToCompleteObjective.ToString()) +
                    " enemies";

        if (string.IsNullOrEmpty(Description))
            Description = GetUpdatedCounterAmount();
    }

    void OnEnemyKilled(EnemyKillEvent evt)
    {
        if (IsCompleted)
            return;

        killTotal++;

        if (MustKillAllEnemies)
            KillsToCompleteObjective = evt.RemainingEnemyCount + killTotal;

        int targetRemaining = MustKillAllEnemies ? evt.RemainingEnemyCount : KillsToCompleteObjective - killTotal;

        // update the objective text according to how many enemies remain to kill
        if (targetRemaining == 0)
        {
            StartCoroutine(EndWave());
        }
        else if (targetRemaining == 1)
        {
            string notificationText = NotificationEnemiesRemainingThreshold >= targetRemaining
                ? "One enemy left"
                : string.Empty;
            UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
        }
        else
        {
            // create a notification text if needed, if it stays empty, the notification will not be created
            string notificationText = NotificationEnemiesRemainingThreshold >= targetRemaining
                ? targetRemaining + " enemies to kill left"
                : string.Empty;

            UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
        }
    }

    IEnumerator EndWave()
    {
        yield return new WaitForSeconds(endWaveDelay);
        spawnManager.EndWave();
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