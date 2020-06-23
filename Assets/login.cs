using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Facebook.Unity;
using System.Threading.Tasks;
using Firebase.Extensions;
using System;
using UnityEngine.SceneManagement;

public class login : MonoBehaviour
{
    [SerializeField]
    GameObject email;

    [SerializeField]
    GameObject pass;

    
    //Guest Login working well
    async public void GuestLogin()
    {

        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    

        await auth.SignInWithEmailAndPasswordAsync(email.GetComponent<UnityEngine.UI.Text>().text, pass.GetComponent<UnityEngine.UI.Text>().text).ContinueWith(task =>
        {

            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }

            string token = task.Result.Email;

            if (token.Length > 0)
            {

                FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
                DocumentReference docRef = db.Collection("users").Document(auth.CurrentUser.UserId);

                docRef.GetSnapshotAsync().ContinueWithOnMainThread(tassk =>
                {
                    
                    DocumentSnapshot snapshot = tassk.Result;
                    if (snapshot.Exists)
                    {
                        Debug.Log(String.Format("Document data for {0} document:", snapshot.Id));
                        Dictionary<string, object> city = snapshot.ToDictionary();
                        foreach (KeyValuePair<string, object> pair in city)
                        {
                            authy.username = pair.Value.ToString();
                            Debug.Log(String.Format("{0}: {1}", pair.Key, pair.Value));
                        }
                        Debug.Log(authy.username);
                        authy.uid = auth.CurrentUser.UserId;
                        SceneManager.LoadScene(1, LoadSceneMode.Single);
                    }
                });
            }


        });
    }

    //All of this is facebook, zzz
    void Awake()
    {
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }
    }
    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }
    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }
    public void FBlogin()
    {
        List<string> permissions = new List<string>();
        permissions.Add("public_profile");
        permissions.Add("email");
        permissions.Add("user_friends");
        FB.LogInWithReadPermissions(permissions, AuthCallback);
    }
    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            List<string> permissions = new List<string>();

            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log(aToken.TokenString);
            Debug.Log(aToken.UserId);


            Firebase.Auth.Credential credential = Firebase.Auth.FacebookAuthProvider.GetCredential(aToken.TokenString);
            auth.SignInWithCredentialAsync(credential).ContinueWith(async task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInWithCredentialAsync was canceled.");

                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);

                    return;
                }

                Firebase.Auth.FirebaseUser newUser = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                        newUser.DisplayName, newUser.UserId);

                string token = await task.Result.TokenAsync(true);

                if (token.Length > 0)
                {
                    authy.token = token;

                    //string newData = JsonUtility.ToJson(authy, false);
                    
                }
            });
            // Print current access token's granted permissions
            //foreach(string perms in aToken.Permissions)
            //{
            // Debug.Log(perms);
            //}
        }
        else
        {
            Debug.Log("User cancelled login");
        }
    }

    
}

public static class authy
{
    public static string token;
    public static string email;
    public static string username;
    public static string uid;
    public static string version = "1.0.1";
}