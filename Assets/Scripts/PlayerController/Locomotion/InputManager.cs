using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerControls playerControls;
    PlayerLocomotion playerLocomotion;
    AnimatorManager animatorManager;

    public Vector2 movementInput;

    public float moveAmmount;
    public float verticalInput;
    public float horizontalInput;

    public bool sprintInput;

    public void Awake() {
        animatorManager = GetComponent<AnimatorManager>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
    }

    private void OnEnable() {
        if (playerControls == null) {
            playerControls = new PlayerControls();

            //Subscribes an anonymus function to the .performed event. "i" is the parameter that is passed in the function.
            playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();

            playerControls.PlayerActions.Sprint.performed += i => sprintInput = true;
            playerControls.PlayerActions.Sprint.canceled += i => sprintInput = false;
        }

        playerControls.Enable();
    }

    private void OnDisable() {
        playerControls.Disable();
    }

    public void HandleAllInputs() {
        HandleMovementInput();
        HandleSprintingInput();
    }

    private void HandleMovementInput() {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;

        moveAmmount = Mathf.Clamp01(Mathf.Abs(horizontalInput)+Mathf.Abs(verticalInput));
        animatorManager.UpdateAnimatorValues(0, moveAmmount, playerLocomotion.isSprinting);
    }

    private void HandleSprintingInput() {
        if (sprintInput && moveAmmount>0.55f) {
            playerLocomotion.isSprinting= true;
        } else {
            playerLocomotion.isSprinting = false;
        }
    }
}
