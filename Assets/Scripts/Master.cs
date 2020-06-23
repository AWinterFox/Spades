using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Master : MonoBehaviourPunCallbacks
{
    private static Master _instance;

    [SerializeField]
    private string menuScene;

    [SerializeField]
    private string gameScene;

    [SerializeField]
    private string lobbyScene;

    private string currentScene;

    private void Awake()
    {
        if(_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += SceneLoaded;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public static void LoadMenu()
    {
        SceneManager.LoadScene(_instance.menuScene);
    }

    public static void LoadLobby()
    {
        SceneManager.LoadScene(_instance.lobbyScene);
    }

    public static void LoadGame(PlayerInfo[] players)
    {

        _instance.StartCoroutine(_instance.LoadGameI(players));
        PhotonNetwork.LoadLevel(_instance.gameScene);
    }

    private IEnumerator LoadGameI(PlayerInfo[] players)
    {
        Debug.Log("Called");
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("We are master.");
            while(currentScene != gameScene)
            {
                yield return null;
            }

            var manager = FindObjectOfType<GameManager>();

            manager.StartGame(players);
        }
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.name;
    }
}
