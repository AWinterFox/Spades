using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenManager : MonoBehaviour
{
    private const string TOKENS_PREF = "tokens";

    private static TokenManager instance;

    private int tokens;
    public static int Tokens { get { return instance.tokens; } }

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        Load();
    }

    private void Load()
    {
        tokens = PlayerPrefs.GetInt(TOKENS_PREF, 20000);
    }

    private void Save()
    {
        PlayerPrefs.SetInt(TOKENS_PREF, tokens);
    }

    public static void AddTokens(int tokens)
    {
        instance.tokens += tokens;
        instance.Save();
    }

    public static void TakeTokens(int tokens)
    {
        instance.tokens -= tokens;
        instance.Save();
    }
}
