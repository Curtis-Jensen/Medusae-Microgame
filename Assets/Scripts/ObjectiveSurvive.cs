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
        if (evt.RemainingEnemyCount == 0)
            StartCoroutine(spawnManager.EndWave());
    }

    void OnDestroy()
    {
        EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
    }
}