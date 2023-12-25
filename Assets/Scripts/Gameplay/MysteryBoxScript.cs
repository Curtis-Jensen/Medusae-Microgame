using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TO DO: Drop current weapon
public class MysteryBoxScript : MonoBehaviour
{
    [Tooltip("Array of gun prefabs")]
    public GameObject[] weapons;
    public float spawnHeight;

    void Start()
    {
        // TO DO: Have a timer that decides when to spawn
        // Get a random index to pick a gun from the array
        int randomWeaponIndex = Random.Range(0, weapons.Length);

        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y + spawnHeight, transform.position.z);

        // Instantiate the randomly chosen gun at the spawn point
        Instantiate(weapons[randomWeaponIndex], spawnPosition, Quaternion.identity);
    }
}
