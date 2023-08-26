using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDestroyer : MonoBehaviour
{
    [Tooltip("How hard the tile pops up when broken")]
    public float popForce;
    public GameObject tileParent;

    Health health;
    float maxHealth;
    float tileCount;
    float initialTileCount;

    void Start()
    {
        health = GetComponent<Health>();
        maxHealth = health.maxHealth;
        tileCount = tileParent.transform.childCount;
        initialTileCount = tileCount;
    }

    /* Checks the percentage of health and the percentage of breakable tiles.
     * 
     * If there are more breakable tiles than health points, tiles will break
     */
    void FixedUpdate()
    {
        float healthPercentage =  health.currentHealth / maxHealth;
        float tilePercentage = tileCount / initialTileCount;

        if (healthPercentage < tilePercentage) DestroyATile();
    }

    void DestroyATile()
    {
        var allTiles = tileParent.GetComponentsInChildren<Rigidbody>();
        List<Rigidbody> stableTiles = new List<Rigidbody>();

        foreach (var tile in allTiles)
            if (tile.useGravity == false) stableTiles.Add(tile);

        var fallingTile = stableTiles[Random.Range(0, stableTiles.Count)];

        fallingTile.isKinematic = false;
        fallingTile.useGravity = true;
        fallingTile.AddForce(new Vector3(0, popForce, 0), ForceMode.Impulse);
        tileCount--;
    }
}
