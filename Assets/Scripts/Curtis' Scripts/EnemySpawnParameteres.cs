using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemySpawnParameters : ScriptableObject
{
    public GameObject prefab;
    public GameObject spawnLocationParent;
    public float spawningChance;
    public int enemyMagnitude;


    public EnemySpawnParameters(GameObject prefab, GameObject spawnLocationParent, float spawningChance, int enemyMagnitude)
    {

    }
}
