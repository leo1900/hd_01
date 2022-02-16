using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquidGameMainMenuUI : MonoBehaviour {

    private void Awake() {
        transform.Find("PlayBtn").GetComponent<Button>().onClick.AddListener(() => {
            Loader.Load(Loader.Scene.SquidGame_RedLightGreenLight);
        });
        transform.Find("CodeMonkeyBtn").GetComponent<Button>().onClick.AddListener(() => {
            Application.OpenURL("https://youtube.com/c/CodeMonkeyUnity");
        });
    }
}