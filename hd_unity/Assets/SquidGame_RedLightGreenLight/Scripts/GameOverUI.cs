using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour {


    private void Awake() {
        transform.Find("ContinueBtn").GetComponent<Button>().onClick.AddListener(() => {
            Loader.Load(Loader.Scene.SquidGame_MainMenu);
        });
    }

    private void Start() {
        SquidGame_RedLightGreenLight.Instance.OnGameOver += Instance_OnGameOver;

        Hide();
    }

    private void Instance_OnGameOver(object sender, System.EventArgs e) {
        SquidGame_RedLightGreenLight.Stats squidGameStats = SquidGame_RedLightGreenLight.Instance.GetStats();
        transform.Find("Kills").Find("AmountText").GetComponent<TextMeshProUGUI>().text = squidGameStats.playersKilled.ToString();
        transform.Find("WrongKills").Find("AmountText").GetComponent<TextMeshProUGUI>().text = squidGameStats.playersKilledWrong.ToString();
        transform.Find("Escaped").Find("AmountText").GetComponent<TextMeshProUGUI>().text = squidGameStats.playersEscaped.ToString();
        transform.Find("Win").Find("AmountText").GetComponent<TextMeshProUGUI>().text = squidGameStats.playersWin.ToString();
        transform.Find("Shots").Find("AmountText").GetComponent<TextMeshProUGUI>().text = squidGameStats.ammoFired.ToString();
        transform.Find("Hits").Find("AmountText").GetComponent<TextMeshProUGUI>().text = Mathf.Round(SquidGame_RedLightGreenLight.Instance.GetAmmoHitPercent() * 100f) + "%";
        transform.Find("Time").Find("AmountText").GetComponent<TextMeshProUGUI>().text = Mathf.Round(squidGameStats.time) + "s";

        CalculateScore(squidGameStats);

        gameObject.SetActive(true);
    }

    private void CalculateScore(SquidGame_RedLightGreenLight.Stats squidGameStats) {
        float score = 0;
        float hitPercent = SquidGame_RedLightGreenLight.Instance.GetAmmoHitPercent();

        score += squidGameStats.playersKilled * 200 * hitPercent;
        transform.Find("Kills").Find("ScoreText").GetComponent<TextMeshProUGUI>().text = "+" + Mathf.Round(squidGameStats.playersKilled * 100 * hitPercent);

        score -= squidGameStats.playersKilledWrong * 2000;
        transform.Find("WrongKills").Find("ScoreText").GetComponent<TextMeshProUGUI>().text = "-" + Mathf.Round(squidGameStats.playersKilledWrong * 2000);

        score -= squidGameStats.playersEscaped * 500;
        transform.Find("Escaped").Find("ScoreText").GetComponent<TextMeshProUGUI>().text = "-" + Mathf.Round(squidGameStats.playersEscaped * 500);

        score += squidGameStats.playersWin * 1000;
        transform.Find("Win").Find("ScoreText").GetComponent<TextMeshProUGUI>().text = "+" + Mathf.Round(squidGameStats.playersWin * 1000);

        score += (100 - squidGameStats.time) * 50;
        transform.Find("Time").Find("ScoreText").GetComponent<TextMeshProUGUI>().text = "+" + Mathf.Round((100 - squidGameStats.time) * 50);

        transform.Find("Shots").Find("ScoreText").GetComponent<TextMeshProUGUI>().text = "";
        transform.Find("Hits").Find("ScoreText").GetComponent<TextMeshProUGUI>().text = "x" + hitPercent.ToString("F2");

        transform.Find("ScoreText").GetComponent<TextMeshProUGUI>().text = "SCORE: " + Mathf.Round(score);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

}