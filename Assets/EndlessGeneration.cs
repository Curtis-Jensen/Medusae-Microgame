using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessGeneration : MonoBehaviour
{
    public Transform player;
    public Transform lastSpawned;
    public GameObject floor;
    public float loadingDistance = 20;

    void Update()
    {
        if (player.position.y > (lastSpawned.position.y + loadingDistance)) return;

        var newPosition =
            new Vector3(lastSpawned.position.x, lastSpawned.position.y - 100, lastSpawned.position.z);
        GameObject newFloor = Instantiate(floor, newPosition, Quaternion.identity, transform);

        lastSpawned = newFloor.transform;
    }
}
