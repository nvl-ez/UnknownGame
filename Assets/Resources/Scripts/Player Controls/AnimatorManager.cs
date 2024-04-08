using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
    public Animator animator;
    int horizontal;
    int vertical;

    private void Awake() {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;
        horizontal = Animator.StringToHash("Horizontal");
        vertical = Animator.StringToHash("Vertical");
    }

    public void UpdateAnimatorValues(float horizontalMovement, float verticalMovement, bool isSprinting) {

        float snappedHorizontal;
        float snappedVertical;

        if (isSprinting)
        {
            snappedHorizontal = horizontalMovement;
            snappedVertical = 2;
        } else {
            snappedHorizontal = snapper(horizontalMovement);
            snappedVertical = snapper(verticalMovement);
        }

        animator.SetFloat(horizontal, snappedHorizontal, 0.1f, Time.deltaTime);
        animator.SetFloat(vertical, snappedVertical, 0.1f, Time.deltaTime);
    }

    private float snapper(float value) {
        if(Mathf.Abs(value)>0 && Mathf.Abs(value) < 0.55f) {
            return (value / Mathf.Abs(value)) * 0.5f;
        } else if (Mathf.Abs(value) > 0.55f) {
            return (value / Mathf.Abs(value)) * 1;
        }
        return 0;
    }

    public void playTargetAnimation(string targetAnimation, bool isInteracting) {
        animator.SetBool("isInteracting", isInteracting);
        animator.CrossFade(targetAnimation, 0.2f);
    }
}
