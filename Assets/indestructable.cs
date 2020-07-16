﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Object.DontDestroyOnLoad example.
//
// This script example manages the playing audio. The GameObject with the
// "music" tag is the BackgroundMusic GameObject. The AudioSource has the
// audio attached to the AudioClip.

public class indestructable : MonoBehaviour
{
    void Awake()
    {
        GameObject socket = this.gameObject;

        DontDestroyOnLoad(socket);
    }
}