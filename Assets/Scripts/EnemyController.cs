using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.AI;
using Unity.FPS.Game;

public class EnemyController : NpcController
{
    EnemyManager enemyManager;

    void Start()
    {
        base.Start();

        enemyManager = FindObjectOfType<EnemyManager>();
        DebugUtility.HandleErrorIfNullFindObject<EnemyManager, NpcController>(enemyManager, this);

        enemyManager.RegisterEnemy(this);
    }

    void OnDie()
    {
        // Tells the game flow manager to handle the enemy destuction
        enemyManager.UnregisterEnemy(this);

        var player = GameObject.Find("Player");
        player.GetComponent<Health>().Heal(vampirismHeal);

        base.OnDie();
    }
}
