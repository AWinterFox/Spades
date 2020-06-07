using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource music;

    [SerializeField]
    private AudioSource gameOver;

    [SerializeField]
    private AudioSource dealSound;

    [SerializeField]
    private AudioSource spadeBroken;

    [SerializeField]
    private AudioSource cardPlaySound;

    [SerializeField]
    private AudioSource winSound;

    [SerializeField]
    private AudioSource loseGame;

    private void Start()
    {
        var manager = FindObjectOfType<GameManager>();

        manager.OnCardPlayed.AddListener(() =>
        {
            cardPlaySound.Play();
        });

        manager.OnStateChanged.AddListener((state) =>
        {
            switch (state)
            {
                case GameState.ReadyToDeal:
                    break;
                case GameState.Dealing:
                    dealSound.Play();
                    break;
                case GameState.Sorting:
                    break;
                case GameState.Bid:
                    break;
                case GameState.Trick:
                    break;
                case GameState.Calculating:
                    break;
                case GameState.Finished:
                    gameOver.Play();
                    break;
                default:
                    break;
            }
        });

        manager.OnSpadesBroken.AddListener(() =>
        {
            spadeBroken.Play();
        });

        manager.OnGameLost.AddListener(() =>
        {
            loseGame.Play();
        });

        manager.OnGameWon.AddListener(() =>
        {
            winSound.Play();
        });

    }

    public void SetSoundActive(bool active)
    {
        music.volume = active ? 1 : 0;
        dealSound.volume = active ? 1 : 0;
        gameOver.volume = active ? 1 : 0;
        spadeBroken.volume = active ? 1 : 0;
        loseGame.volume = active ? 1 : 0;
        winSound.volume = active ? 1 : 0;
    }
}
