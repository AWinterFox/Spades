using SocketIO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using Firebase;

public class socketManager : MonoBehaviour
{
    [SerializeField]
    public GameObject SocketIOP;

    public static socketManager Current { get; private set; }
    SocketIOComponent socket;
    // Start is called before the first frame update
    void Start()
    {
        socketManager.Current = this;
        socket = SocketIOP.GetComponent<SocketIOComponent>();

        socket.On("connect", Connected);
    }

    public void Emit(string room, string json, bool loginstuff, string jsonn = null)
    {

        if (loginstuff)
        {
            socket.Emit(room, new JSONObject("ff"));
        }
        else
        {
            socket.Emit(room, new JSONObject(json));
        }

    }

    void update(SocketIOEvent e)
    {
    }

    void Connected(SocketIOEvent e)
    {
        Debug.Log("Connected");
    }

    public void TestClose(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
        //ResetGameData.resetData();
        SceneManager.LoadScene(1, LoadSceneMode.Single);

    }

    //bad stuff
    public void TestError(SocketIOEvent e)
    {
        Debug.LogError(e.data[9].ToString());
        Application.Quit();
    }
}

[Serializable]
public class SocketHandShake
{
    public SocketMessage[] Message;
}

[Serializable]
public class SocketMessage
{
    public string id;
    public string message;
    public int connected;
}

