using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using System.Timers;

public class showAd : MonoBehaviour
{
    #if UNITY_ANDROID
            string gameId = "3594942";
    #elif UNITY_IPHONE
        string gameId = "3594943";
    #else
        string gameId = "3594942";
    #endif

    string myPlacementId = "rewardedVideo";
    bool testMode = false;
    private static Timer timer;


    // Initialize the Ads listener and service:
    void Start()
    {
        Debug.Log("Kapow");
        Advertisement.Initialize(gameId, testMode);
    }

    // Implement IUnityAdsListener interface methods:
    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        // Define conditional logic for each ad completion status:
        if (showResult == ShowResult.Finished)
        {
            Debug.Log("Completed");
            

            // Reward the user for watching the ad to completion.
        }
        else if (showResult == ShowResult.Skipped)
        {
            // Do not reward the user for skipping the ad.
        }
        else if (showResult == ShowResult.Failed)
        {
            Debug.LogWarning("The ad did not finish due to an error.");
        }
    }

    public void OnUnityAdsReady(string placementId)
    {
        // If the ready Placement is rewarded, show the ad:
        if (placementId == myPlacementId)
        {
            timer = new System.Timers.Timer(10000);
            // Hook up the Elapsed event for the timer.
            timer.Elapsed += OnTimedEvent;
            timer.Enabled = true;
            Advertisement.Show(myPlacementId);
        }
    }

    private static void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        timer.Stop();
        TokenManager.AddTokens(100);
    }

    public void OnUnityAdsDidError(string message)
    {
        // Log the error.
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        // Optional actions to take when the end-users triggers an ad.
    }
}