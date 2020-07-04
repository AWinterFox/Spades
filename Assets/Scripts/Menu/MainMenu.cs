using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine.Advertisements;
using UnityEngine.Purchasing;
using System;

public class MainMenu : MonoBehaviour
{

    [SerializeField]
    private TMP_Text tokenTotal;

    [SerializeField]
    private GameObject panel;

    [SerializeField]
    private GameObject bonus;

    [SerializeField]
    private AudioSource music;

    [SerializeField]
    private Lobby lobby;

    string gameId = "3594942";
    bool testMode = true;    

    void Start()
    {
        Advertisement.Initialize(gameId, testMode);
    }

    private void Awake()
    {
        music.Play();
        tokenTotal.text = (TokenManager.Tokens).ToString();

      

        var lastLogin = PlayerPrefs.GetString("daily", System.DateTime.Now.ToString());

        var last = System.DateTime.Parse(lastLogin);
        var current = System.DateTime.Now;
        

        if (current.Ticks - last.Ticks > 864000000000)
        {
            bonus.SetActive(true);
            TokenManager.AddTokens(1000);
            PlayerPrefs.SetString("daily", System.DateTime.Now.ToString());
        }

        //if (Names == null)
        //{
        //    var filePath = Path.Combine(Application.streamingAssetsPath, "names.json");

        //    if (File.Exists(filePath))
        //    {
        //        var fs = new FileStream(filePath, FileMode.Open);
        //        var sr = new StreamReader(fs);

        //        Names = new List<string> { "Hello", "bye" };
        //        var tempJson = JsonConvert.SerializeObject(Names);

        //        var json = sr.ReadToEnd();

        //        Names = JsonConvert.DeserializeObject<List<string>>(json);
        //    }
        //}
    }
   

    public void OnPlay(int score)
    {
        if(score == 150){
            GameManager.TokenBet = 500;
        }else if(score == 300){
            GameManager.TokenBet = 1000;
        }else if(score == 500){
            GameManager.TokenBet = 1500;
        }

        if(GameManager.TokenBet < TokenManager.Tokens){
            GameManager.WinScore = 10;//score;
            GameManager.Tournament = null;
            lobby.StartLobby();
        }else{
            panel.SetActive(true);
        }
    }

    public void OnTournament()
    {
        GameManager.TokenBet = 5000;
        GameManager.WinScore = 150;

        var playerTeam = new Team
        {
            Player1 = "You",
            Player2 = NameGen.GetAiName()
        };

        
        var tournament = new Tournament
        {
            PlayerTeam = playerTeam,
            Teams = new List<Team>
            {
                playerTeam,
                new Team
                {
                    Player1 = NameGen.GetAiName(), Player2 = NameGen.GetAiName()
                },
                new Team
                {
                    Player1 = NameGen.GetAiName(), Player2 = NameGen.GetAiName()
                },
                new Team
                {
                    Player1 = NameGen.GetAiName(), Player2 = NameGen.GetAiName()
                },
                new Team
                {
                    Player1 = NameGen.GetAiName(), Player2 = NameGen.GetAiName()
                },
                new Team
                {
                    Player1 = NameGen.GetAiName(), Player2 = NameGen.GetAiName()
                },
                new Team
                {
                    Player1 = NameGen.GetAiName(), Player2 = NameGen.GetAiName()
                },
                new Team
                {
                    Player1 = NameGen.GetAiName(), Player2 = NameGen.GetAiName()
                }
            }
        };
        tournament.Setup();
        var bracket = tournament.Rounds.First().First(b => b.Team1 == tournament.PlayerTeam || b.Team2 == tournament.PlayerTeam);

        var otherTeam = bracket.Team1 == tournament.PlayerTeam ? bracket.Team2 : bracket.Team1;

        //TODO: Fix tournament to new moddle.

        //GameManager.PlayerNames = new List<string>
        //{
        //    tournament.PlayerTeam.Player1,
        //    otherTeam.Player1,
        //    tournament.PlayerTeam.Player2,
        //    otherTeam.Player2
        //};

        //GameManager.Tournament = tournament;

        //SceneManager.LoadScene("Game");
    }

    private void Update()
    {
        tokenTotal.text = TokenManager.Tokens.ToString();
    }
}

public class Tournament
{
    public List<List<Bracket>> Rounds = new List<List<Bracket>>();
    public List<Team> Teams;
    public Team PlayerTeam;

    public void Setup()
    {
        List<Bracket> round1 = new List<Bracket>();
        //Teams.OrderBy(t => t.Player2);

        for (int i = 0; i < 4; i++)
        {
            var index = i * 2;
            var bracket = new Bracket
            {
                Team1 = Teams[index],
                Team2 = Teams[index + 1]
            };

            round1.Add(bracket);
        }

        Rounds.Add(round1);
    }
}

public class Bracket
{
    public Team Team1;
    public Team Team2;
    public Team Winner;
}

public class Team
{
    public string Player1;
    public string Player2;
}