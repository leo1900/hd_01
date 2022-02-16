using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using HighlightPlus;
using CodeMonkey.Utils;

public class SquidGamePlayer : MonoBehaviour {

    public event EventHandler OnDead;
    public event EventHandler OnEscaped;
    public event EventHandler OnWin;
    

    public enum State {
        Idle,
        RunningForward,
        RunningBack,
        RunAway,
        Dead,
        Escaped,
        Win,
    }

    [SerializeField] private Transform pfRagdoll;
    //[SerializeField] private HighlightProfile greenHighlightProfile;
    //[SerializeField] private HighlightProfile redHighlightProfile;
    [SerializeField] private float winZ;

    private State state;
    private Animator animator;
    //private HighlightEffect highlightEffect;
    private HealthSystem healthSystem;
    private float moveSpeed;
    private SquidGameDoor squidGameDoor;
    private Vector3 doorHammerPosition;
    private Vector3 doorEscapePosition;
    private bool isRedHighlight;
    private float doorDamageTimer;

    private void Awake() {
        animator = GetComponent<Animator>();
        //highlightEffect = GetComponent<HighlightEffect>();

        moveSpeed = 3.5f + UnityEngine.Random.Range(0f, +1.0f);
        animator.SetFloat("SpeedMultiplier", moveSpeed / 3.5f);

        healthSystem = new HealthSystem(100);
        healthSystem.OnDead += HealthSystem_OnDead;

        HideHighlight();

        state = State.Idle;
    }

    private void Update() {
        switch (state) {
            case State.Idle:
                animator.SetBool("IsRunning", false);
                break;
            case State.RunningForward:
                // Run forward
                transform.position += transform.forward * moveSpeed * Time.deltaTime;

                animator.SetBool("IsRunning", true);

                if (transform.position.z > winZ) {
                    // Win!
                    state = State.Win;

                    HideHighlight();
                    animator.SetBool("IsRunning", false);

                    OnWin?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.RunningBack:
                // Run to door
                if (Vector3.Distance(transform.position, doorHammerPosition) > .7f) {
                    // Rotate to face door
                    Vector3 runningBackDir = (doorHammerPosition - transform.position).normalized;
                    float rotationSpeed = 10f;
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(runningBackDir), rotationSpeed * Time.deltaTime);

                    // Run forward
                    transform.position += transform.forward * moveSpeed * Time.deltaTime;

                    animator.SetBool("IsRunning2", true);
                } else {
                    // Arrived at door position
                    if (squidGameDoor.IsDead()) {
                        // Door destroyed
                        state = State.RunAway;
                    } else {
                        animator.SetTrigger("Hammer");
                        //animator.SetBool("IsRunning2", false);

                        doorDamageTimer -= Time.deltaTime;
                        if (doorDamageTimer <= 0f) {
                            float doorDamageTimerMax = .5f;
                            doorDamageTimer += doorDamageTimerMax;
                            squidGameDoor.Damage();
                        }
                    }
                }
                break;
            case State.RunAway:
                if (Vector3.Distance(transform.position, doorEscapePosition) > 1f) {
                    Vector3 runningAwayDir = (doorEscapePosition - transform.position).normalized;
                    float runAwayRotationSpeed = 10f;
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(runningAwayDir), runAwayRotationSpeed * Time.deltaTime);

                    // Run forward
                    transform.position += transform.forward * moveSpeed * Time.deltaTime;

                    animator.SetBool("IsRunning2", true);
                } else {
                    state = State.Escaped;

                    Destroy(gameObject);

                    OnEscaped?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.Win:
                // Win!
                break;
        }
    }

    private void HealthSystem_OnDead(object sender, System.EventArgs e) {
        Transform ragdollTransform = Instantiate(pfRagdoll, transform.position, transform.rotation);
        //Destroy(ragdollTransform.gameObject, 3f);

        MatchAllChildTransforms(transform, ragdollTransform);

        Vector3 explosionPosition =
            transform.position +
            transform.forward +
            transform.right * UnityEngine.Random.Range(-3f, +3f) +
            new Vector3(0, UnityEngine.Random.Range(.0f, 1.5f), 0);

        ApplyExplosionToRagdoll(ragdollTransform, 3000f, explosionPosition, 0f);

        state = State.Dead;

        Destroy(gameObject);

        OnDead?.Invoke(this, EventArgs.Empty);
    }

    public void Damage() {
        healthSystem.Damage(100);
    }

    public bool IsDead() {
        return state == State.Dead;
    }

    private void MatchAllChildTransforms(Transform root, Transform clone) {
        foreach (Transform child in root) {
            Transform cloneChild = clone.Find(child.name);
            if (cloneChild != null) {
                cloneChild.position = child.position;
                cloneChild.rotation = child.rotation;

                MatchAllChildTransforms(child, cloneChild);
            }
        }
    }

    private void ApplyExplosionToRagdoll(Transform root, float explosionForce, Vector3 explosionPosition, float explosionRange) {
        foreach (Transform child in root) {
            if (child.TryGetComponent<Rigidbody>(out Rigidbody childRigidbody)) {
                childRigidbody.AddExplosionForce(explosionForce, explosionPosition, explosionRange);
            }

            ApplyExplosionToRagdoll(child, explosionForce, explosionPosition, explosionRange);
        }
    }

    public void HideHighlight() {
        //highlightEffect.enabled = false;
        isRedHighlight = false;
    }

    public void ShowGreenHighlight() {
        //highlightEffect.ProfileLoad(greenHighlightProfile);
        //highlightEffect.enabled = true;
        isRedHighlight = false;
    }

    public void ShowRedHighlight() {
        //highlightEffect.ProfileLoad(redHighlightProfile);
        //highlightEffect.enabled = true;
        isRedHighlight = true;
    }

    public bool IsRedHighlight() {
        return isRedHighlight;
    }

    public void StartRunningBack(float delay, float speedMultiplier, SquidGameDoor squidGameDoor) {
        FunctionTimer.Create(() => {
            if (IsDead()) return;
            this.squidGameDoor = squidGameDoor;
            doorHammerPosition = squidGameDoor.transform.Find("point").position;
            doorHammerPosition += new Vector3(UnityEngine.Random.Range(-1.7f, +1.7f), 0, 0);
            doorEscapePosition = squidGameDoor.transform.Find("point").position + new Vector3(0, 0, -2);
            moveSpeed *= speedMultiplier; // Run back at different speed
            animator.SetFloat("SpeedMultiplier", moveSpeed / 3.5f / 1.0f);
            state = State.RunningBack;
        }, delay);
    }

    public void StartRunningForward(float delay) {
        FunctionTimer.Create(() => {
            if (IsDead()) return;
            state = State.RunningForward;
        }, delay);
    }

    public void StopRunning(float delay) {
        FunctionTimer.Create(() => {
            if (IsDead()) return;
            state = State.Idle;
        }, delay);
    }

}