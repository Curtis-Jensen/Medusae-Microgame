using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DivingDistance : MonoBehaviour
{
    public CharacterController player;

    void Update()
    {
        if (player.velocity.y >= 0f)
            Debug.Log("Player is not moving downward.");
    }
}
