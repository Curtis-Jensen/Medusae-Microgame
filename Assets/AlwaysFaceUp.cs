using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysFaceUp : MonoBehaviour
{
    void Start()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
    }
}
