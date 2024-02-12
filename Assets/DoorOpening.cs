using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpening : MonoBehaviour
{
    public GameObject door;

    void OnTriggerEnter(Collider other)
    {
        door.SetActive(false);
    }
}
