using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Audio;
using Cinemachine;
using CodeMonkey.Utils;

public class SquidGame_RedLightGreenLight : MonoBehaviour {

    public static SquidGame_RedLightGreenLight Instance { get; private set; }

    public event EventHandler OnPlayerDead;
    public event EventHandler OnPlayerEscaped;
    public event EventHandler OnPlayerWin;
    public event EventHandler OnAmmoFired;
    public event EventHandler OnGameOver;


    private enum State {
        Start,
        GreenLight,
        RedLight,
        GameOver,
    }

    [SerializeField] private Transform zoomCameraTransform;
    [SerializeField] private Transform pfDebugSpawn;
    [SerializeField] private Transform pfShootGround;
    [SerializeField] private Transform pfShootBlood;
    [SerializeField] private LayerMask playersLayerMask;
    [SerializeField] private Transform pfPlayerMale;
    [SerializeField] private Transform pfPlayerFemale;
    [SerializeField] private Transform spawnPlayersAreaMin;
    [SerializeField] private Transform spawnPlayersAreaMax;
    [SerializeField] private Transform door1;
    [SerializeField] private Transform door2;
    [SerializeField] private Transform door3;
    [SerializeField] private Animator hitMarkerAnimator;
    [SerializeField] private Animator dollAnimator;
    [SerializeField] private Volume depthOfFieldVolume;
    [SerializeField] private Transform[] pfAudioGunshotArray;
    [SerializeField] private AudioSource redLightGreenLightAudioSource;
    [SerializeField] private AudioMixerGroup redLightGreenLightMixerGroup_1;
    [SerializeField] private AudioMixerGroup redLightGreenLightMixerGroup_2;
    [SerializeField] private AudioMixerGroup redLightGreenLightMixerGroup_3;
    [SerializeField] private AudioMixerGroup redLightGreenLightMixerGroup_4;
    [SerializeField] private AudioMixerGroup redLightGreenLightMixerGroup_5;
    [SerializeField] private Animator shotVolumeAnimator;

    private Camera zoomCamera;
    private CinemachineImpulseSource cinemachineImpulseSource;
    private State state;
    private List<SquidGamePlayer> playerList;
    private List<SquidGamePlayer> redPlayerList;
    private int stageNumber;
    private float shootTimer;
    private float shootTimerMax = .2f;
    private int ammoFired;
    private int ammoHit;
    private int playersKilled;
    private int playersKilledWrong;
    private int playersEscaped;
    private int playersWin;
    private float time;

    private void Awake() {
        Instance = this;

        zoomCamera = zoomCameraTransform.GetComponent<Camera>();
        zoomCamera.enabled = false;

        cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
        state = State.RedLight;

        playerList = new List<SquidGamePlayer>();
        redPlayerList = new List<SquidGamePlayer>();
    }

    private void Start() {
        SpawnPlayers();

        FunctionTimer.Create(SetGreenLight, 1f);
    }

    private void Update() {
        switch (state) {
            default:
                HandleShooting();
                time += Time.deltaTime;
                break;
            case State.GameOver:
                // Do nothing
                break;
        }

    }

    private void HandleShooting() {
        zoomCameraTransform.LookAt(Mouse3D.GetMouseWorldPosition());

        zoomCamera.enabled = Input.GetMouseButton(1);

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f) {
            if (Input.GetMouseButton(0) && Input.GetMouseButton(1)) {
                shootTimer = shootTimerMax;
                Shoot();
            }
        }
    }

    private void Shoot() {
        ammoFired++;
        cinemachineImpulseSource.GenerateImpulse();
        PlayAudioGunshot();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, playersLayerMask)) {
            ammoHit++;
            hitMarkerAnimator.SetTrigger("Hit");
            shotVolumeAnimator.SetTrigger("Hit");

            SquidGamePlayer squidGamePlayer = raycastHit.transform.GetComponent<SquidGamePlayer>();
            squidGamePlayer.Damage();

            Vector3 dirToCamera = (Camera.main.transform.position - raycastHit.point).normalized;

            Transform bloodTransform = Instantiate(pfShootBlood, raycastHit.point, Quaternion.identity);
            Vector3 randomVector = new Vector3(
                UnityEngine.Random.Range(-1f, +1f),
                UnityEngine.Random.Range(-1f, +1f),
                UnityEngine.Random.Range(-1f, +1f)
            );
            bloodTransform.LookAt(raycastHit.point + (dirToCamera * 10f) + randomVector * UnityEngine.Random.Range(4f, 8f));
        } else {
            Instantiate(pfShootGround, Mouse3D.GetMouseWorldPosition(), Quaternion.identity);
        }

        OnAmmoFired?.Invoke(this, EventArgs.Empty);
    }

    private void PlayAudioGunshot() {
        Transform pfAudioGunshot = pfAudioGunshotArray[UnityEngine.Random.Range(0, pfAudioGunshotArray.Length)];
        Destroy(Instantiate(pfAudioGunshot).gameObject, 2f); // Instantiate Shot sound and clean up afterwards
    }

    private void SpawnPlayers() {
        playerList = new List<SquidGamePlayer>();
        for (int i = 0; i < 100; i++) {
            Transform pfPlayer = UnityEngine.Random.Range(0, 100) < 50 ? pfPlayerMale : pfPlayerFemale;

            Vector3 spawnPosition = new Vector3(
                UnityEngine.Random.Range(spawnPlayersAreaMin.position.x, spawnPlayersAreaMax.position.x),
                0f,
                UnityEngine.Random.Range(spawnPlayersAreaMin.position.z, spawnPlayersAreaMax.position.z));

            Transform playerTransform = Instantiate(pfPlayer, spawnPosition, Quaternion.identity);

            SquidGamePlayer squidGamePlayer = playerTransform.GetComponent<SquidGamePlayer>();
            squidGamePlayer.OnDead += SquidGamePlayer_OnDead;
            squidGamePlayer.OnEscaped += SquidGamePlayer_OnEscaped;
            squidGamePlayer.OnWin += SquidGamePlayer_OnWin;
            playerList.Add(squidGamePlayer);
        }
    }

    private void SquidGamePlayer_OnWin(object sender, EventArgs e) {
        RemovePlayer(sender as SquidGamePlayer);

        playersWin++;

        OnPlayerWin?.Invoke(this, EventArgs.Empty);
    }

    private void SquidGamePlayer_OnEscaped(object sender, EventArgs e) {
        RemovePlayer(sender as SquidGamePlayer);

        playersEscaped++;

        OnPlayerEscaped?.Invoke(this, EventArgs.Empty);
    }

    private void SquidGamePlayer_OnDead(object sender, EventArgs e) {
        SquidGamePlayer squidGamePlayer = sender as SquidGamePlayer;
        if (squidGamePlayer.IsRedHighlight()) {
            playersKilled++;
        } else {
            // Guard shot a Green highlight, wrong
            playersKilledWrong++;
        }

        RemovePlayer(squidGamePlayer);

        OnPlayerDead?.Invoke(this, EventArgs.Empty);
    }

    private void RemovePlayer(SquidGamePlayer squidGamePlayer) {
        playerList.Remove(squidGamePlayer);

        if (state == State.RedLight && redPlayerList.Count > 0) {
            redPlayerList.Remove(squidGamePlayer);
            if (redPlayerList.Count <= 0) {
                FunctionTimer.Create(SetGreenLight, 1f);
            }
        }
    }

    public int GetPlayerCount() {
        return playerList.Count;
    }

    public int GetAmmoFired() {
        return ammoFired;
    }

    public float GetAmmoHitPercent() {
        return (float)ammoHit / Mathf.Max(1, ammoFired);
    }

    public float GetTime() {
        return time;
    }

    public struct Stats {
        public int ammoFired;
        public int ammoHit;
        public int playersKilled;
        public int playersKilledWrong;
        public int playersEscaped;
        public int playersWin;
        public float time;
    }

    public Stats GetStats() {
        return new Stats {
            ammoFired = ammoFired,
            ammoHit = ammoHit,
            playersKilled = playersKilled,
            playersKilledWrong = playersKilledWrong,
            playersEscaped = playersEscaped,
            playersWin = playersWin,
            time = time,
        };
    }

    private void SetGreenLight() {
        state = State.GreenLight;
        
        if (dollAnimator != null) dollAnimator.SetBool("LookBack", false);

        stageNumber++;
        Debug.Log("Stage Number: " + stageNumber);


        switch (stageNumber) {
            case 1: redLightGreenLightAudioSource.outputAudioMixerGroup = redLightGreenLightMixerGroup_1; break;
            case 2: redLightGreenLightAudioSource.outputAudioMixerGroup = redLightGreenLightMixerGroup_2; break;
            case 3: redLightGreenLightAudioSource.outputAudioMixerGroup = redLightGreenLightMixerGroup_3; break;
            case 4: redLightGreenLightAudioSource.outputAudioMixerGroup = redLightGreenLightMixerGroup_4; break;
            case 5:
            default:
                redLightGreenLightAudioSource.outputAudioMixerGroup = redLightGreenLightMixerGroup_5;
                break;
        }

        switch (stageNumber) {
            case 1: redLightGreenLightAudioSource.pitch = 1.2f; break;
            case 2: redLightGreenLightAudioSource.pitch = 1.1f; break;
            case 3: redLightGreenLightAudioSource.pitch = 1.0f; break;
            case 4: redLightGreenLightAudioSource.pitch = 1.2f; break;
            case 5:
            default:
                redLightGreenLightAudioSource.pitch = 1.0f;
                break;
        }

        redLightGreenLightAudioSource.Play();


        float manualDelay = 0f;

        switch (stageNumber) {
            case 1: manualDelay = 1f; break;
            case 2: manualDelay = .5f; break;
            case 3: manualDelay = .4f; break;
            case 4: manualDelay = .0f; break;
            case 5:
            default:
                manualDelay = .0f;
                break;
        }

        foreach (SquidGamePlayer squidGamePlayer in playerList) {
            float startRunningDelay = manualDelay + UnityEngine.Random.Range(.3f, 1f);
            squidGamePlayer.StartRunningForward(startRunningDelay);

            squidGamePlayer.HideHighlight();
        }

        float redLightTimer = manualDelay + 3f - stageNumber * .2f;
        redLightTimer = Mathf.Max(1.5f, redLightTimer);
        FunctionTimer.Create(SetRedLight, redLightTimer);
    }

    private void SetRedLight() {
        state = State.RedLight;

        if (dollAnimator != null) dollAnimator.SetBool("LookBack", true);

        int minRedPlayerCount = Mathf.Max(4, playerList.Count / 10);
        int maxRedPlayerCount = Mathf.Min(30, playerList.Count / 2);
        redPlayerList = new List<SquidGamePlayer>();

        if (playerList.Count < 5) {
            minRedPlayerCount = 1;
            maxRedPlayerCount = 1;
        }

        foreach (SquidGamePlayer squidGamePlayer in playerList) {
            bool isSafe = UnityEngine.Random.Range(0, 100) < 50;

            if (redPlayerList.Count < minRedPlayerCount) {
                isSafe = false;
            }
            if (redPlayerList.Count >= maxRedPlayerCount) {
                isSafe = true;
            }

            float stopRunningDelay;
            if (isSafe) {
                stopRunningDelay = UnityEngine.Random.Range(0, .1f);
                squidGamePlayer.StopRunning(stopRunningDelay);
            } else {
                // Find closest door
                Transform closestDoor = door1;
                if (Vector3.Distance(squidGamePlayer.transform.position, door2.position) < Vector3.Distance(squidGamePlayer.transform.position, closestDoor.position)) {
                    closestDoor = door2;
                }
                if (Vector3.Distance(squidGamePlayer.transform.position, door3.position) < Vector3.Distance(squidGamePlayer.transform.position, closestDoor.position)) {
                    closestDoor = door3;
                }

                float speedMultiplier = 1f;
                speedMultiplier += stageNumber * .2f;
                speedMultiplier = Mathf.Min(speedMultiplier, 2f);

                // Chance to run to door immediately
                bool runToDoorImmediately = UnityEngine.Random.Range(0, 100) < 50;
                if (runToDoorImmediately) {
                    // Run to closest door
                    stopRunningDelay = UnityEngine.Random.Range(.0f, .3f);
                    squidGamePlayer.StartRunningBack(stopRunningDelay, speedMultiplier, closestDoor.GetComponent<SquidGameDoor>());
                } else {
                    // Stop running too late but stay still
                    stopRunningDelay = UnityEngine.Random.Range(.1f, .3f);
                    squidGamePlayer.StopRunning(stopRunningDelay);

                    // Force run back after some time
                    float runBackTime = UnityEngine.Random.Range(1f + maxRedPlayerCount * .15f, 3f + maxRedPlayerCount * .3f);
                    FunctionTimer.Create(() => {
                        squidGamePlayer.StartRunningBack(0f, speedMultiplier, closestDoor.GetComponent<SquidGameDoor>());
                    }, runBackTime);
                }
            }

            if (isSafe) {
                squidGamePlayer.ShowGreenHighlight();
            } else {
                squidGamePlayer.ShowRedHighlight();
                redPlayerList.Add(squidGamePlayer);
            }
        }

        if (redPlayerList.Count == 0) {
            // No more red players!
            // Game Over!
            Debug.Log("Game Over!");

            state = State.GameOver;
            depthOfFieldVolume.weight = 1f;

            OnGameOver?.Invoke(this, EventArgs.Empty);
        }
    }

}