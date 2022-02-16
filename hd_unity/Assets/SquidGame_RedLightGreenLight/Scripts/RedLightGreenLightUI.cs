using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RedLightGreenLightUI : MonoBehaviour {

    private TextMeshProUGUI playersText;
    private TextMeshProUGUI ammoText;
    private TextMeshProUGUI hitText;
    private TextMeshProUGUI timeText;

    private void Awake() {
        playersText = transform.Find("playersText").GetComponent<TextMeshProUGUI>();
        ammoText = transform.Find("ammoText").GetComponent<TextMeshProUGUI>();
        hitText = transform.Find("hitText").GetComponent<TextMeshProUGUI>();
        timeText = transform.Find("timeText").GetComponent<TextMeshProUGUI>();
    }

    private void Start() {
        SquidGame_RedLightGreenLight.Instance.OnPlayerDead += Instance_OnPlayerDead;
        SquidGame_RedLightGreenLight.Instance.OnAmmoFired += Instance_OnAmmoFired;

        UpdateUI();
    }

    private void Update() {
        timeText.text = Mathf.Round(SquidGame_RedLightGreenLight.Instance.GetTime()) + "s";
    }

    private void Instance_OnAmmoFired(object sender, System.EventArgs e) {
        UpdateUI();
    }

    private void Instance_OnPlayerDead(object sender, System.EventArgs e) {
        UpdateUI();
    }

    private void UpdateUI() {
        playersText.text = SquidGame_RedLightGreenLight.Instance.GetPlayerCount().ToString();
        ammoText.text = SquidGame_RedLightGreenLight.Instance.GetAmmoFired().ToString();
        hitText.text = Mathf.Round(SquidGame_RedLightGreenLight.Instance.GetAmmoHitPercent() * 100f) + "%";
    }

}