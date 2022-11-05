using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip[] musicChoices;
    AudioSource source;

    void Start()
    {
        source = gameObject.GetComponent<AudioSource>();
        StartCoroutine(PlayMusic());
    }

    IEnumerator PlayMusic()
    {
        source.clip = musicChoices[Random.Range(0, musicChoices.Length)];
        source.Play();
        yield return new WaitForSeconds(source.clip.length);
        source.Stop();
        StartCoroutine(PlayMusic());
    }
}