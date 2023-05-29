using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Tooltip("How long it takes to do one full day")]
    [Min(1)]
    public float secondsInDay;

    Transform sunTransform;
    float dayTime;

    void Start()
    {
        sunTransform = GetComponent<Transform>();
    }

    void Update()
    {
        SunRotation();
    }

    /* Advances time and rotates sun
     */
    void SunRotation()
    {
        dayTime += Time.deltaTime;

        sunTransform.rotation =
            Quaternion.Euler(new Vector3(dayTime / secondsInDay * 360, 0, 0));
    }
}
