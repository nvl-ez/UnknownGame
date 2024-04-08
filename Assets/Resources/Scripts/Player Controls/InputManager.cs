using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerControls playerControls;
    PlayerLocomotion playerLocomotion;
    AnimatorManager animatorManager;
 
    public Vector2 movementInput;
    public float moveAmount;
    public float verticalInput;
    public float horizontalInput;

    public bool sprintingInput;
    public bool jumpInput;

    private void Awake() {
        animatorManager = GetComponent<AnimatorManager>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
    }

    private void OnEnable() {
        if (playerControls == null) { 
            playerControls = new PlayerControls();
            playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();

            playerControls.PlayerActions.Sprint.performed += i => sprintingInput = true;
            playerControls.PlayerActions.Sprint.canceled += i => sprintingInput = false;

            playerControls.PlayerActions.Jump.performed += i => jumpInput = true;
        }

        playerControls.Enable();
    }

    private void OnDisable() {
        playerControls.Disable();
    }

    public void handleAllInputs() {
        handleMovementInput();
        handleSprintingInput();
        handleJumpingInput();
    }


    private void handleMovementInput() {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;

        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));
        animatorManager.UpdateAnimatorValues(0, moveAmount, playerLocomotion.isSprinting);
    }

    private void handleSprintingInput() {
        playerLocomotion.isSprinting = sprintingInput && (moveAmount>0.55f);
    }

    private void handleJumpingInput() {
        if (jumpInput) {
            jumpInput = false;
            playerLocomotion.handleJumping();
        }
    }
}
