using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTilting : MonoBehaviour
{
    public float tiltIntensity = 8f;

    void Awake()
    {
        float randomAngleX = transform.localRotation.x + Random.Range(-tiltIntensity, tiltIntensity);
        float randomAngleY = transform.localRotation.y + Random.Range(-tiltIntensity, tiltIntensity);
        float randomAngleZ = transform.localRotation.z + Random.Range(-tiltIntensity, tiltIntensity);

        Vector3 randomRotation = new Vector3(randomAngleX, randomAngleY, randomAngleZ);
        transform.localRotation = Quaternion.Euler(randomRotation);
    }
}
