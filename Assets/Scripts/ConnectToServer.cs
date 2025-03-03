using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks {
    void Awake() {
        PhotonNetwork.SendRate = 30;           // packets per second
        PhotonNetwork.SerializationRate = 30;  // serialization calls per second
    }
    
    void Start() {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby() {
        SceneManager.LoadScene("Lobby");
    }
}
