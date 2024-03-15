using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    GravityBody gravityBody;
    PlayerManager playerManager;
    AnimatorManager animatorManager;
    InputManager inputManager;

    Vector3 moveDirection;
    Transform cameraObject;
    Rigidbody playerRidbody;

    [Header("Falling")]
    public float leapingVelocity;
    public LayerMask groundLayer;
    public float rayCastHeightOffset = 0.5f;

    [Header("Movement Flags")]
    public bool isGrounded;
    public bool isSprinting;

    [Header("Movement Speeds")]
    public float walkingSpeed = 1.5f;
    public float runningSpeed = 5;
    public float sprintingSpeed = 7;
    public float rotationSpeed = 15;

    private void Awake() {
        inputManager = GetComponent<InputManager>();
        playerManager = GetComponent<PlayerManager>();
        gravityBody = gameObject.GetComponent<GravityBody>();
        animatorManager = GetComponent<AnimatorManager>();
        playerRidbody = GetComponent<Rigidbody>();
        cameraObject = Camera.main.transform;
    }

    public void HandleAllMovement() {
        HandleFallingAndLanding();
        if (playerManager.isInteracting) return;
        HandleMovement();
        //HandleRotation();
    }

    private void HandleMovement() {
        moveDirection = cameraObject.forward * inputManager.verticalInput;
        moveDirection = moveDirection+cameraObject.right*inputManager.horizontalInput;
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
        //targetDirection.y = 0;

        if (targetDirection == Vector3.zero) targetDirection = transform.forward;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed*Time.deltaTime);

        transform.rotation = playerRotation;
    }
    Vector3 hitPoint;
    private void HandleFallingAndLanding() {
        RaycastHit hit;
        Vector3 rayCastOrigin = transform.position;
        rayCastOrigin.y = rayCastOrigin.y + rayCastHeightOffset;

        if(!isGrounded) {
            if(!playerManager.isInteracting) {
                animatorManager.PlayerTargetAnimation("Falling", true);
            }
            playerRidbody.AddForce(transform.forward * leapingVelocity);
        }

        if (Physics.SphereCast(rayCastOrigin, 0.2f, gravityBody.planet.transform.position, out hit, 1f,groundLayer)) {
            hitPoint = hit.point;
            Debug.Log("HIT");
            if(!isGrounded && !playerManager.isInteracting) {
                animatorManager.PlayerTargetAnimation("Land", true);
            }

            isGrounded = true;
        } else {
            isGrounded = false;
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitPoint, 0.2f);
    }

    private void FixedUpdate() {
        Vector3 rayCastOrigin = transform.localPosition;
        rayCastOrigin.y =  rayCastOrigin.y + rayCastHeightOffset;
        rayCastOrigin = this.transform.TransformDirection(rayCastOrigin);
        Debug.DrawLine(rayCastOrigin, gravityBody.planet.transform.position, Color.red);
    }
}
