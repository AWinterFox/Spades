using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOptionsUi : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField]
    private Button activeButton;

    [SerializeField]
    private Button quitButton;

    [SerializeField]
    private Button resumeButton;

    [Header("Objects")]
    [SerializeField]
    private GameObject menu;

    [SerializeField]
    private Toggle soundToggle;

    [SerializeField]
    private SoundManager soundManager;

    private void Awake()
    {
        menu.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        activeButton.onClick.AddListener(OnActive);
        quitButton.onClick.AddListener(OnQuit);
        resumeButton.onClick.AddListener(OnResume);

        soundToggle.onValueChanged.AddListener(OnSoundToggled);
    }

    private void OnDisable()
    {
        activeButton.onClick.RemoveListener(OnActive);
        quitButton.onClick.RemoveListener(OnQuit);
        resumeButton.onClick.RemoveListener(OnResume);

        soundToggle.onValueChanged.RemoveListener(OnSoundToggled);
    }

    private void OnActive()
    {
        menu.gameObject.SetActive(true);
    }

    private void OnQuit()
    {
        var gameManager = FindObjectOfType<GameManager>();

        gameManager.LoseGame(true);

        Photon.Pun.PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Menu");
    }

    private void OnResume()
    {
        menu.gameObject.SetActive(false);
    }

    private void OnSoundToggled(bool active)
    {
        soundManager.SetSoundActive(active);
    }
}
