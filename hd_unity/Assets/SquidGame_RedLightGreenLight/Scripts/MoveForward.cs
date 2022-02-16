using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveForward : MonoBehaviour {

    private Animator animator;
    private float moveSpeed;

    private void Awake() {
        animator = GetComponent<Animator>();

        moveSpeed = 2.5f + Random.Range(-.5f, +.5f);
    }

    private void Update() {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        animator.SetBool("IsRunning", true);
    }

}