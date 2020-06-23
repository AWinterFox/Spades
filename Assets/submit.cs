using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using Firebase.Extensions;
using System;

public class submit : MonoBehaviour
{

    [SerializeField]
    GameObject username;

    [SerializeField]
    GameObject register;

    [SerializeField]
    GameObject username2;

    [SerializeField]
    GameObject email;

    [SerializeField]
    GameObject password;

    [SerializeField]
    GameObject cpassword;

    bool change = false;
    // Start is called before the first frame update

    public static submit Current { get; private set; }
    void Start()
    {
        submit.Current = this;
    }

     void Update()
    {
        if (change)
        {
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }
    }

    public void showMe()
    {
        register.SetActive(true);
    }

    public void submitData()
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        CollectionReference citiesRef = db.Collection("users");
        citiesRef.Document(authy.uid).SetAsync(new Dictionary<string, object>(){
                            { "username", username.GetComponent<UnityEngine.UI.Text>().text }
                        });

        authy.username = username.GetComponent<UnityEngine.UI.Text>().text;
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    async public void rregister()
    {
        string feedme = (username2.GetComponent<UnityEngine.UI.Text>().text);
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        await auth.CreateUserWithEmailAndPasswordAsync(email.GetComponent<UnityEngine.UI.Text>().text, password.GetComponent<UnityEngine.UI.Text>().text).ContinueWith(task =>
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

             Debug.Log("what");




            

             sendStuff(feedme);

             /*ContinueWith(tasker => {
                 
             });*/






         });
    }

    public async void sendStuff(string feedme)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        DocumentReference docRef = db.Collection("users").Document(auth.CurrentUser.UserId);

        Dictionary<string, object> initialData = new Dictionary<string, object>
            {
                { "username", feedme }
            };
        
        try
        {
            await docRef.SetAsync(initialData);
            authy.username = feedme;
            change = true;
        } catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

}

[FirestoreData]
public class Profile
{
    [FirestoreProperty]
    public string username { get; set; }
    
}
