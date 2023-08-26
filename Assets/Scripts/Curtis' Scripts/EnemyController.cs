using UnityEngine;

public class EnemyController : NpcController
{
    [Tooltip("How much the enemies will heal the player when killed")]
    public float vampirismHeal = 1f;

    EnemyManager enemyManager;

    void Start()
    {
        base.Start();

        enemyManager = FindObjectOfType<EnemyManager>();
        DebugUtility.HandleErrorIfNullFindObject<EnemyManager, NpcController>(enemyManager, this);
        enemyManager.RegisterEnemy(this);

        health.OnDie += OnDie;
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
