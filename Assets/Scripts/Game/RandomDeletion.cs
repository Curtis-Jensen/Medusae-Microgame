using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomDeletion : MonoBehaviour
{
    [Range(0,1)]
    [Tooltip("Percentage chance for deletion")]
    public float deletionChance;

    private void Start()
    {
        float randomValue = Random.Range(0f, 1f); // Generate a random value between 0 and 1

        if (randomValue <= deletionChance)
            Destroy(gameObject); // Delete the game object
    }
}
