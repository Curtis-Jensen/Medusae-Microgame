using System.Collections;
using Unity.FPS.Game;
using UnityEngine;

public class ObjectiveSurvive : Objective
{
    public SpawnManager spawnManager;

    protected override void Start()
    {
        base.Start();

        EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);
    }

    void OnEnemyKilled(EnemyKillEvent evt)
    {
        // When there are no enemies left, end the wave
        if (evt.RemainingEnemyCount == 0)
        {
            try
            {
                StartCoroutine(spawnManager.EndWave());
            }
            catch
            {
                Debug.Log("For some reason spawnManager (for enemies) was probably set to null after respawning the player.");
            }
        }
    }
}