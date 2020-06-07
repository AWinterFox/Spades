using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameInfoUi : MonoBehaviour
{
    [SerializeField]
    private TMP_Text bet;

    [SerializeField]
    private TMP_Text team1Bid;

    [SerializeField]
    private TMP_Text team1Score;

    [SerializeField]
    private TMP_Text team2Bid;

    [SerializeField]
    private TMP_Text team2Score;

    [SerializeField]
    private TMP_Text game;

    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        gameManager.OnStateChanged.AddListener(OnGameStateChanged);
    }

    private void OnGameStateChanged(GameState state)
    {
        team1Bid.text = $"Bid: {gameManager.Players[0].Bid + gameManager.Players[2].Bid}";
        team1Score.text = $"Score: {gameManager.Team1.ScoreTotal}";

        team2Bid.text = $"Bid: {gameManager.Players[1].Bid + gameManager.Players[3].Bid}";
        team2Score.text = $"Score: {gameManager.Team2.ScoreTotal}";

        game.text = $"Game: {GameManager.WinScore}";

        bet.text = $"Bet: {GameManager.TokenBet}";
    }
}
