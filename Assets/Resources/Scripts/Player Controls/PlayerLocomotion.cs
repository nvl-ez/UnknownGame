using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PlayerLocomotion : MonoBehaviour
{
    PlayerManager playerManager;
    InputManager inputManager;
    AnimatorManager animatorManager;

    [HideInInspector]
    public Vector3 moveDirection = Vector3.zero;
    [HideInInspector]
    public Vector3 targetDirection = Vector3.zero;

    Transform cameraObject;
    Rigidbody playerRigidbody;
    GravityBody gravityBody;

    [Header("Falling")]
    public float leapingVelocity;
    public LayerMask groundLayer;

    [Header("Speeds")]
    public float walkingSpeed = 6;
    public float runningSpeed = 8;
    public float sprintingSpeed = 11;
    public float rotationSpeed = 15;
    public float jumpHeight = 3;

    [Header("Movement Flags")]
    public bool isSprinting = false;
    public bool isGrounded = false;
    public bool isJumping = false;


    private void Awake() {
        cameraObject = Camera.main.transform;
        inputManager = GetComponent<InputManager>();
        playerRigidbody = GetComponent<Rigidbody>();
        gravityBody = GetComponent<GravityBody>();
        playerManager = GetComponent<PlayerManager>();
        animatorManager = GetComponent<AnimatorManager>();

        playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        playerRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void handleAllMovement() {
        handleFallingAndLanding();
        handleMovement();
        handleRotation();   
    }

    private void handleMovement() {
        // Get the gravity direction from your GravityBody script.
        Vector3 gravityDirection = gravityBody.GravityDirection;

        Vector3 currentVerticalVelocity = Vector3.Project(playerRigidbody.velocity, gravityDirection);

        // Project the camera's forward and right vectors onto a plane perpendicular to the gravity direction.
        Vector3 forwardOnPlane = Vector3.ProjectOnPlane(cameraObject.forward, gravityDirection).normalized;
        Vector3 rightOnPlane = Vector3.ProjectOnPlane(cameraObject.right, gravityDirection).normalized;

        // Calculate the move direction based on player input, using the adjusted forward and right vectors.
        moveDirection = forwardOnPlane * inputManager.verticalInput + rightOnPlane * inputManager.horizontalInput;
        moveDirection.Normalize();

        // Scale the move direction by the movement speed.
        if(isSprinting) {
            moveDirection = moveDirection * sprintingSpeed;
        } else {
            if (inputManager.moveAmount >= 0.55f) {
                moveDirection = moveDirection * runningSpeed;
            } else {
                moveDirection = moveDirection * walkingSpeed;
            }
        }
        // Apply the calculated velocity to the player's Rigidbody.
        if(!isGrounded) {
            playerRigidbody.velocity = moveDirection + currentVerticalVelocity;
        } else {
            playerRigidbody.velocity = moveDirection;
        }
    }

    private void handleRotation() {
        Vector3 gravityDirection = gravityBody.GravityDirection;

        Vector3 forwardOnPlane = Vector3.ProjectOnPlane(cameraObject.forward, gravityDirection).normalized;
        Vector3 rightOnPlane = Vector3.ProjectOnPlane(cameraObject.right, gravityDirection).normalized;
        targetDirection = forwardOnPlane * inputManager.verticalInput + rightOnPlane * inputManager.horizontalInput;
        targetDirection.Normalize();

        if (targetDirection == Vector3.zero) {
            // Use the camera's forward direction or any other reference direction that makes sense in your context.
            Vector3 referenceDirection = cameraObject.forward;
            // Project the reference direction on the plane defined by the gravity direction to get a valid direction on the plane.
            targetDirection = Vector3.ProjectOnPlane(referenceDirection, gravityDirection).normalized;

            // Ensure the target direction is not zero after projection. If it is, use the transform's forward direction as a fallback.
            if (targetDirection == Vector3.zero) {
                targetDirection = transform.forward;
            }
        }


        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, -gravityDirection);

        // Smoothly interpolate to the target rotation.
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Apply the calculated rotation to the player.
        transform.rotation = playerRotation;
    }

    Vector3 rayCastOrigin;
    Vector3 targetPosition;

    private void handleFallingAndLanding() {
        RaycastHit hit;
        rayCastOrigin = transform.position;
        rayCastOrigin += gravityBody.GravityDirection * -0.5f;
        targetPosition = transform.position;

        if (!isGrounded && !isJumping) {
            if (!playerManager.isInteracting) {
                animatorManager.playTargetAnimation("FallingLoop", true);
            }
            playerRigidbody.AddForce(gravityBody.force);
        }

        if (Physics.SphereCast(rayCastOrigin, 0.1f, gravityBody.GravityDirection, out hit, 0.55f, groundLayer) && !isJumping) {
            if(!isGrounded && !playerManager.isInteracting) {
                animatorManager.playTargetAnimation("RollForward", true);
            }

            Vector3 rayCastHitPoint = hit.point;
            // Calculate the vector from the current position to the hit point
            Vector3 toHitPoint = rayCastHitPoint - transform.position;
            // Project this vector onto the gravity direction to get the height adjustment
            Vector3 heightAdjustment = Vector3.Project(toHitPoint, gravityBody.GravityDirection);
            // Adjust the target position by the height adjustment along the gravity direction
            targetPosition += heightAdjustment;

            isGrounded = true;
        } else {
            isGrounded = false;
        }

        
        if(isGrounded && !isJumping) {
            if(playerManager.isInteracting || inputManager.moveAmount>0) {
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime / 0.1f);
            } else {
                transform.position = targetPosition;
            }
        }
    }

    private void OnDrawGizmos() {
        if (Application.isPlaying) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPosition, 0.1f);
        }
    }

    public void handleJumping() {
        if (isGrounded && !isJumping) {
            animatorManager.animator.SetBool("isJumping", true);
            animatorManager.playTargetAnimation("Jump", false);

            float jumpVelocityMagnitude = Mathf.Sqrt(2 * GravityBody.GRAVITY_FORCE * jumpHeight);

            // Apply jump velocity in the direction opposite to gravity
            Vector3 jumpVelocity = gravityBody.GravityDirection.normalized * -jumpVelocityMagnitude;

            // Add the jump force to the current velocity, particularly to its vertical component.
            playerRigidbody.velocity += jumpVelocity;

            isJumping = true; // Remember to reset this flag when landing.
        }
    }

    
}
