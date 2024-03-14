using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    InputManager inputManager;

    Vector3 moveDirection;
    Transform cameraObject;
    Rigidbody playerRidbody;

    public bool isSprinting;

    [Header("Movement Speeds")]
    public float walkingSpeed = 1.5f;
    public float runningSpeed = 5;
    public float sprintingSpeed = 7;
    public float rotationSpeed = 15;

    private void Awake() {
        inputManager = GetComponent<InputManager>();
        playerRidbody = GetComponent<Rigidbody>();
        cameraObject = Camera.main.transform;
    }

    public void HandleAllMovement() {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement() {
        moveDirection = cameraObject.forward * inputManager.verticalInput;
        moveDirection = moveDirection+cameraObject.right*inputManager.horizontalInput;
        moveDirection.y = 0;
        moveDirection.Normalize();

        if(isSprinting ) {
            moveDirection = moveDirection*sprintingSpeed;
        } else {
            if (inputManager.moveAmmount >= 0.55f) {
                moveDirection = moveDirection * runningSpeed;
            } else {
                moveDirection = moveDirection * walkingSpeed;
            }
        }

        

        moveDirection *= runningSpeed;

        Vector3 movementVelocity = moveDirection;
        playerRidbody.velocity = movementVelocity;
    }

    private void HandleRotation() {
        Vector3 targetDirection = Vector3.zero;
        targetDirection = cameraObject.forward*inputManager.verticalInput;
        targetDirection += cameraObject.right * inputManager.horizontalInput;
        targetDirection.Normalize();
        targetDirection.y = 0;

        if (targetDirection == Vector3.zero) targetDirection = transform.forward;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed*Time.deltaTime);

        transform.rotation = playerRotation;
    }
}
