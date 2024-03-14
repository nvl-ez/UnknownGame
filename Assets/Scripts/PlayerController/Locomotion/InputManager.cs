using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerControls playerControls;
    AnimatorManager animatorManager;

    public Vector2 movementInput;
    public Vector2 cameraInput;

    public float cameraInputX;
    public float cameraInputY;

    public float moveAmmount;
    public float verticalInput;
    public float horizontalInput;

    public bool sprintInput;

    public void Awake() {
        animatorManager = GetComponent<AnimatorManager>();
    }

    private void OnEnable() {
        if (playerControls == null) {
            playerControls = new PlayerControls();

            //Subscribes an anonymus function to the .performed event. "i" is the parameter that is passed in the function.
            playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
            playerControls.PlayerMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();
        }

        playerControls.Enable();
    }

    private void OnDisable() {
        playerControls.Disable();
    }

    public void HandleAllInputs() {
        HandleMovementInput();
    }

    private void HandleMovementInput() {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;

        cameraInputX = cameraInput.x;
        cameraInputY = cameraInput.y;

        moveAmmount = Mathf.Clamp01(Mathf.Abs(horizontalInput)+Mathf.Abs(verticalInput));
        animatorManager.UpdateAnimatorValues(0, moveAmmount);
    }
}
