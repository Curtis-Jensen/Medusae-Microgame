using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessGeneration : MonoBehaviour
{
    public Transform player;
    public Transform lastSpawned;
    public GameObject floor;
    public GameObject treeFloor;
    public float loadingDistance = 20;
    public int floorsUntilTree = 20;

    int floorsSpawned;

    void Update()
    {
        if (player.position.y > (lastSpawned.position.y + loadingDistance)) return; // If the player hasn't fallen far enough, don't make a new floor

        if (floorsSpawned == floorsUntilTree)
            SpawnTree();
        else
            SpawnNewFloor();
    }

    void SpawnNewFloor()
    {
        var newPosition =
            new Vector3(lastSpawned.position.x, lastSpawned.position.y - 100, lastSpawned.position.z); // Calculate where the new floor will go
                GameObject newFloor = Instantiate(floor, newPosition, Quaternion.identity, transform); // Spawn the new floor

        lastSpawned = newFloor.transform; // Keep track of where the last floor was to know where the next one goes

        floorsSpawned++;
    }

    void SpawnTree()
    {
        var newPosition =
            new Vector3(lastSpawned.position.x, lastSpawned.position.y - 100, lastSpawned.position.z); // Calculate where the new floor will go
        GameObject newFloor = Instantiate(treeFloor, newPosition, Quaternion.identity, transform); // Spawn the new floor

        lastSpawned = newFloor.transform; // Keep track of where the last floor was to know where the next one goes

        floorsSpawned++;
    }
}
