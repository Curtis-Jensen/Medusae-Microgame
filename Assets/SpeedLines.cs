using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedLines : MonoBehaviour
{
    public float speedThreshold = 30f; // Adjust this value to your desired threshold

    ParticleSystem speedLinesParticleSystem;
    Vector3 previousPosition;
    bool creatingParticles = false;

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

        //Debug.Log(speedLinesParticleSystem.isPlaying);

        if (playerSpeed < speedThreshold)
        {
            // Turn on the particle system
            if (!creatingParticles)
            {
                speedLinesParticleSystem.Play();
                creatingParticles = true;
            }
        }
        else
        {
            // Turn off the particle system
            if (creatingParticles)
            {
                speedLinesParticleSystem.Stop();
                creatingParticles = false;
            }
        }

        // Update the previous position for the next frame
        previousPosition = transform.position;
    }

    float CalculateFallingSpeed()
    {
        // Calculate the distance traveled in the vertical axis
        float verticalDistance = transform.position.y - previousPosition.y;

        // Calculate falling speed using time.deltaTime to normalize the value
        float fallingSpeed = verticalDistance / Time.deltaTime;

        Debug.Log(fallingSpeed);

        return fallingSpeed;
    }
}
