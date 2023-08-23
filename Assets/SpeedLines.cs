using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedLines : MonoBehaviour
{
    public float speedThreshold = 30f; // Adjust this value to your desired threshold
    public float minFOV = 60f; // Adjust the minimum FOV value
    public float maxFOV = 90f; // Adjust the maximum FOV value
    public float smoothingFactor = 0.1f; // Adjust this factor for smoothing
    public Camera cam;

    ParticleSystem speedLinesParticleSystem;
    float smoothedSpeed = 0f;
    Vector3 previousPosition;
    bool fallingFast = false;

    void Start()
    {
        // Initialize the previous position to the player's starting position
        previousPosition = transform.position;
    
        speedLinesParticleSystem = gameObject.GetComponent<ParticleSystem>();
    }

    void Update()
    {
        // Calculate the player's falling speed
        float playerSpeed = CalculateFallingSpeed();

        if (playerSpeed < speedThreshold)
        {
            // Turn on the particle system
            if (!fallingFast)
            {
                speedLinesParticleSystem.Play();
                fallingFast = true;
            }
        }
        else
        {
            // Turn off the particle system
            if (fallingFast)
            {
                speedLinesParticleSystem.Stop();
                fallingFast = false;
            }
        }

        // Smooth the falling speed value
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, playerSpeed, smoothingFactor);

        // Calculate FOV based on smoothed falling speed
        float targetFOV = Mathf.Lerp(minFOV, maxFOV, smoothedSpeed / speedThreshold);

        // Apply the target FOV to the camera's field of view
        cam.fieldOfView = targetFOV;

        // Update the previous position for the next frame
        previousPosition = transform.position;
    }

    float CalculateFallingSpeed()
    {
        // Calculate the distance traveled in the vertical axis
        float verticalDistance = transform.position.y - previousPosition.y;

        // Calculate falling speed using time.deltaTime to normalize the value
        float fallingSpeed = verticalDistance / Time.deltaTime;

        return fallingSpeed;
    }
}
