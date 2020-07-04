using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using Photon.Pun;

public class ScoringScreen : MonoBehaviourPun
{
    [SerializeField]
    private TMP_Text team1CombinedBid;

    [SerializeField]
    private TMP_Text team2CombinedBid;

    [SerializeField]
    private TMP_Text team1TricksTaken;

    [SerializeField]
    private TMP_Text team2TricksTaken;

    [SerializeField]
    private TMP_Text team1Bags;

    [SerializeField]
    private TMP_Text team2Bags;

    [SerializeField]
    private TMP_Text team1Stucks;

    [SerializeField]
    private TMP_Text team2Stucks;

    [SerializeField]
    private TMP_Text team1SuccessfulBid;

    [SerializeField]
    private TMP_Text team2SuccessfulBid;

    [SerializeField]
    private TMP_Text team1BagScore;

    [SerializeField]
    private TMP_Text team2BagScore;

    [SerializeField]
    private TMP_Text team1Total;

    [SerializeField]
    private TMP_Text team2Total;

    [Header("Buttons")]
    [SerializeField]
    private Button playAgainButton;

    [SerializeField]
    private Button playAgainBlindButton;

    [SerializeField]
    private Button resetButton;

    [SerializeField]
    private Button mainMenuButton;

    [SerializeField]
    private Button nextBracketButton;

    [SerializeField]
    private GameObject winnerLabel;

    [SerializeField]
    private TMP_Text winnerText;

    private GameManager manager;

    private void Awake()
    {
        manager = FindObjectOfType<GameManager>();

        playAgainButton.onClick.AddListener(() => {
            TurnOff();
            manager.NewGame();
        });
        playAgainBlindButton.onClick.AddListener(() => {
            TurnOff();
            manager.NewGame();
        });
        resetButton.onClick.AddListener(() =>
        {
            TurnOff();
            manager.ResetGame();
        });
        mainMenuButton.onClick.AddListener(() =>
        {
            GameManager.GotoMainMenu();
        });

        nextBracketButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Game");
        });


        mainMenuButton.gameObject.SetActive(false);
        nextBracketButton.gameObject.SetActive(false);


    }

    [PunRPC]
    public void TurnOnScoreScreen(Result team1, Result team2)
    {
        //Team 1
        var r1 = team1;

        team1Bags.text = r1.Bags.ToString();
        team1Stucks.text = r1.GamesWithOutTricks.ToString();
        team1BagScore.text = r1.BagsScore.ToString();
        team1BagsPrevious.text = r1.BagsPreviousRound.ToString();
        team1CombinedBid.text = r1.Bid.ToString();
        team1Score.text = r1.Score.ToString();
        team1ScorePrevious.text = r1.ScoreLastRound.ToString();
        team1SuccessfulBid.text = r1.TricksScore.ToString();
        team1Total.text = r1.ScoreTotal.ToString();
        team1TricksTaken.text = r1.Tricks.ToString();
        team1TotalBags.text = r1.BagsTotal.ToString();

        var r2 = team2;

        team2Bags.text = r2.Bags.ToString();
        team2Stucks.text = r2.GamesWithOutTricks.ToString();
        team2BagScore.text = r2.BagsScore.ToString();
        team2BagsPrevious.text = r2.BagsPreviousRound.ToString();
        team2CombinedBid.text = r2.Bid.ToString();
        team2ScorePrevious.text = r2.ScoreLastRound.ToString();
        team2Score.text = r2.Score.ToString();
        team2SuccessfulBid.text = r2.TricksScore.ToString();
        team2Total.text = r2.ScoreTotal.ToString();
        team2TricksTaken.text = r2.Tricks.ToString();
        team2TotalBags.text = r2.BagsTotal.ToString();

        //playAgainBlindButton.gameObject.SetActive(r1.ScoreTotal < r2.ScoreTotal - 99);

        if (r1.ScoreTotal >= GameManager.WinScore || r2.Lost)
        {
            winnerLabel.SetActive(true);
            winnerText.text = "Team 1 Wins";
            //if (GameManager.CurrentBracket != null)
            //{
            //    GameManager.CurrentBracket.Winner = GameManager.CurrentBracket.Team1;
            //}

            //var tournament = GameManager.Tournament;
            //if (GameManager.Tournament != null)
            //{
            //    var currentRound = GameManager.Tournament.Rounds.Last();

            //    if (currentRound.Count < 1)
            //    {
            //        winnerText.text = "Team 1 Wins the tournament";
            //        SetEndButton(false);
            //    }
            //    else
            //    {
            //        SetEndButton(true);
            //        var newRound = new List<Bracket>();
            //        for (int i = 0; i < currentRound.Count / 2; i++)
            //        {
            //            var index = i;
            //            var bracket1 = currentRound[index];
            //            var bracket2 = currentRound[index + 2];

            //            if (bracket1.Winner == null)
            //            {
            //                bracket1.Winner = Random.Range(0, 2) == 0 ? bracket1.Team1 : bracket1.Team2;
            //            }
            //            if (bracket2.Winner == null)
            //            {
            //                bracket2.Winner = Random.Range(0, 2) == 0 ? bracket2.Team1 : bracket2.Team2;
            //            }

            //            var newBracket = new Bracket
            //            {
            //                Team1 = bracket1.Winner,
            //                Team2 = bracket2.Winner
            //            };

            //            if (newBracket.Team2 == GameManager.Tournament.PlayerTeam)
            //            {
            //                newBracket.Team2 = newBracket.Team1;
            //                newBracket.Team1 = GameManager.Tournament.PlayerTeam;
            //            }


            //            newRound.Add(newBracket);
            //        }
            //        Debug.Log("Bracket: " + newRound);
            //        GameManager.Tournament.Rounds.Add(newRound);

            //        var bracket = tournament.Rounds.First().First(b => b.Team1 == tournament.PlayerTeam || b.Team2 == tournament.PlayerTeam);
            //        var otherTeam = bracket.Team1 == tournament.PlayerTeam ? bracket.Team2 : bracket.Team1;

            //        //GameManager.PlayerNames = new List<string>
            //        //{
            //        //    tournament.PlayerTeam.Player1,
            //        //    otherTeam.Player1,
            //        //    tournament.PlayerTeam.Player2,
            //        //    otherTeam.Player2
            //        //};


            //    }
            //}
            //else
            //{
                
            //}

        }
        else if (r1.Lost)
        {
            winnerLabel.SetActive(true);
            winnerText.text = "Team 2 Wins";
            //if (GameManager.CurrentBracket != null)
            //{
            //    GameManager.CurrentBracket.Winner = GameManager.CurrentBracket.Team2;
            //}

            //SetEndButton(true);
        }
        else if (r2.ScoreTotal >= GameManager.WinScore || r1.Lost)
        {
            winnerLabel.SetActive(true);
            winnerText.text = "Team 1 Wins";
            //if (GameManager.CurrentBracket != null)
            //{
            //    GameManager.CurrentBracket.Winner = GameManager.CurrentBracket.Team2;
            //}

            //SetEndButton(false);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("TurnOnScoreScreen", RpcTarget.Others);

            SetEndButton(true);

        }

        gameObject.SetActive(true);
    }

    [PunRPC]
    public void TurnOff()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("TurnOff", RpcTarget.Others);
        }

        gameObject.SetActive(false);
    }

    private void SetEndButton(bool win)
    {
        if(GameManager.Tournament != null && win)
        {
            mainMenuButton.gameObject.SetActive(false);
            nextBracketButton.gameObject.SetActive(true);
        }
        else
        {
            mainMenuButton.gameObject.SetActive(true);
            nextBracketButton.gameObject.SetActive(false);
            playAgainBlindButton.gameObject.SetActive(false);
        }
        
        playAgainButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);
    }
}
