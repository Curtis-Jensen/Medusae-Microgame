using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

public class TileDestroyer : MonoBehaviour
{
    public GameObject tileParent;

    Health health;
    float maxHealth;
    int tileCount;
    float initialTileCount;

    void Start()
    {
        health = GetComponent<Health>();
        maxHealth = health.MaxHealth;
        tileCount = tileParent.transform.childCount;
        initialTileCount = tileCount;
    }

    /* Checks the percentage of health and the percentage of breakable tiles.
     * 
     * If there are more breakable tiles than health points, tiles will break
     */
    void FixedUpdate()
    {
        float healthPercentage =  health.CurrentHealth / maxHealth;
        float tilePercentage = (float)tileParent.transform.childCount / initialTileCount;

        if (healthPercentage < tilePercentage) DestroyATile();
    }

    void DestroyATile()
    {
        var children = tileParent.GetComponentsInChildren<Transform>();

        DestroyImmediate(children[Random.Range(1, children.Length)].gameObject);
    }
}
