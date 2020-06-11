using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Lobby : MonoBehaviourPunCallbacks
{
    const string GAME_VERSION = "1";
    const float HOST_TIME = 10;

    [SerializeField]
    private GameObject panel;

    private bool hosting = false;
    private float hostingElapsed = 0;

    private List<PlayerInfo> players = new List<PlayerInfo>();

    private void Awake()
    {
#if UNITY_EDITOR
            //PhotonNetwork.OfflineMode = true;
#endif
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = "1";
        }
    }

    private void Update()
    {
        if (hosting)
        {
            hostingElapsed += Time.deltaTime;

            if(hostingElapsed > HOST_TIME)
            {

                while(players.Count < 4)
                {
                    players.Add(new PlayerInfo
                    {
                        Name = "AI",
                        Type = PlayerType.Ai
                    });
                }

                Master.LoadGame(players.ToArray());
                hosting = false;
            }
        }
    }

    public void StartLobby()
    {
        hosting = false;
        StartCoroutine(StartLobbyI());
    }

    private IEnumerator StartLobbyI()
    {
        panel.SetActive(true);

        while(!PhotonNetwork.IsConnectedAndReady)
        {
            yield return null;
        }

        if (PhotonNetwork.OfflineMode)
        {
            CreateRoom();
        }
        else
        {
           PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Joined Room failed");
        CreateRoom();
    }

    private void CreateRoom()
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions
        {
            MaxPlayers = 4
        });
        
        hosting = true;
    }

    public override void OnCreatedRoom()
    {
        players.Add(new PlayerInfo
        {
            Name = "You",
            NetworkPlayer = PhotonNetwork.LocalPlayer,
            Type = PlayerType.ActivePlayer
        });
        hosting = true;
        hostingElapsed = 0;
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log("Player entered");
        players.Add(new PlayerInfo
        {
            Name = newPlayer.NickName,
            Type = PlayerType.ActivePlayer,
            NetworkPlayer = newPlayer,
        });
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        players = players.Where(p => p.NetworkPlayer != otherPlayer).ToList();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
    }
}
