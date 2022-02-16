using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouseUI : MonoBehaviour {

    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Update() {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, null, out Vector2 localPoint);
        GetComponent<RectTransform>().localPosition = localPoint;

        if (Input.GetMouseButtonDown(1)) {
            animator.SetBool("ScopeUp", true);
            Cursor.visible = false;
        }

        if (Input.GetMouseButtonUp(1)) {
            animator.SetBool("ScopeUp", false);
            Cursor.visible = true;
        }
    }

}