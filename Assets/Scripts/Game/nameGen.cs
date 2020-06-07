using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class nameGen : MonoBehaviour
{
    [SerializeField]
    private TMP_Text player1;

    [SerializeField]
    private TMP_Text player2;

    [SerializeField]
    private TMP_Text player3;

   
    // Start is called before the first frame update
    void Start()
    {
        string[] names = new string[] { "Aaren", "Aarika", "Abagael", "Abagail", "Abbe", "Abbey", "Abbi", "Abbie", "Abby", "Abbye", "Abigael", "Abigail", "Abigale", "Abra", "Ada", "Adah" };
        player1.text = names[UnityEngine.Random.Range(0,10)];
        player2.text = names[UnityEngine.Random.Range(0, 10)];
        player3.text = names[UnityEngine.Random.Range(0, 10)];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

