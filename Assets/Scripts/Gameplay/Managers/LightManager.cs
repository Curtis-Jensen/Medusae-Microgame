using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class LightManager : MonoBehaviour
    {
        public float maxRange;
        public float maxIntensity;
        public float maxSpotAngle;

        Light playerLight;

        void Start()
        {
            playerLight = GetComponent<Light>();
        }

        public void GainLight()
        {
            playerLight.range = maxRange;
            playerLight.intensity = maxIntensity;
            playerLight.spotAngle = maxSpotAngle;
        }
    }
}