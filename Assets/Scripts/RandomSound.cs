using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSound : MonoBehaviour
{
    [Tooltip("What audio clips could possibly be attached to the audio source")]
    public AudioClip[] audioClips;

    void Awake()
    {
        var audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
        audioSource.Play();
    }
}
