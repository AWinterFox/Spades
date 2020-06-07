using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class TournnamentScreen : MonoBehaviour
{
    [SerializeField]
    private List<TMP_Text> round1Names;

    [SerializeField]
    private List<TMP_Text> round2Names;

    [SerializeField]
    private List<TMP_Text> round3Names;

    [SerializeField]
    private TMP_Text winnerName;

    [SerializeField]
    private Button continueButton;

    private void OnEnable()
    {
        var tournament = GameManager.Tournament;

        foreach (var item in round1Names)
        {
            item.gameObject.SetActive(false);
        }

        foreach (var item in round2Names)
        {
            item.gameObject.SetActive(false);
        }

        foreach (var item in round3Names)
        {
            item.gameObject.SetActive(false);
        }

        winnerName.gameObject.SetActive(false);

        for (int i = 0; i < tournament.Rounds.Count; i++)
        {
            var round = tournament.Rounds[i];
            var names = round1Names;
            if (i == 1) names = round2Names;
            if (i == 2) names = round3Names;
            for (int r = 0; r < round.Count; r++)
            {
                var textIndex = r * 2;
                names[textIndex].text = $"{round[r].Team1.Player1} and {round[r].Team1.Player2}";
                names[textIndex+1].text = $"{round[r].Team2.Player1} and {round[r].Team2.Player2}";

                names[textIndex].gameObject.SetActive(true);
                names[textIndex+1].gameObject.SetActive(true);
            }

            var bracket = tournament.Rounds.Last().First().Winner;
            if (i == 2 && bracket != null)
            {
                winnerName.gameObject.SetActive(true);
                winnerName.text = $"{bracket.Player1} and {bracket.Player2}";
            }
        }

        continueButton.onClick.AddListener(OnContinue);
    }

    private void OnDisable()
    {
        continueButton.onClick.RemoveListener(OnContinue);
    }

    private void OnContinue()
    {
        gameObject.SetActive(false);
    }
}
