using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public GameObject player1;
    public GameObject player2;
    public void Singleplayer()
    {
        PlayGame(new Rect(0, 0, 1, 1), false);
    }

    public void SplitScreen()
    {
        PlayGame(new Rect(0, .5f, 1, .5f), true);
    }

    void PlayGame(Rect screenSpace, bool twoPlayer)
    {
        player2.SetActive(twoPlayer);
        player1.GetComponentInChildren<Camera>().rect = screenSpace;
    }
}
