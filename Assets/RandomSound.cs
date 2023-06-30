using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSound : MonoBehaviour
{
    [Tooltip("What audio clips could possibly be attached to the audio source")]
    public AudioClip[] audioClips;

    void Start()
    {
        gameObject.GetComponent<AudioSource>().clip = audioClips[Random.Range(0, audioClips.Length)];
    }
}
