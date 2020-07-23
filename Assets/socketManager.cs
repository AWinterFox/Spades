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
    }

    public void On(string key, Action<SocketIOEvent> action)
    {
        socket.On(key, action);
    }

    public void Off(string key, Action<SocketIOEvent> action)
    {
        socket.Off(key, action);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="room"></param>
    /// <param name="data"></param>
    /// <param name="loginstuff"></param>
    /// <param name="jsonn"></param>
    public void Emit<T>(string room, T data, bool loginstuff, string jsonn = null)
    {
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        socket.Emit(room, new JSONObject(json));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="room"></param>
    /// <param name="loginstuff"></param>
    /// <param name="jsonn"></param>
    public void Emit(string room, bool loginstuff, string jsonn = null)
    {
        socket.Emit(room);
    }

    void update(SocketIOEvent e)
    {

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

