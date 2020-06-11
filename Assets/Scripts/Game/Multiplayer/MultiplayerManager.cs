using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spades.Multiplayer
{
    public class MultiplayerManager : MonoBehaviourPunCallbacks
    {
        private static MultiplayerManager _instance;
        const string GAME_VERSION = "1";


        private void Awake()
        {
            if(_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            ConnectToPhoton();
        }

        void Start()
        {
        
        }

        private void ConnectToPhoton()
        {
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = GAME_VERSION;
            }
        }

        #region Callbacks

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);
        }

        #endregion
    }
}
