using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    InputManager inputManager;

    public Transform targetTransform;//The object that the camera will follow
    public Transform cameraPivot;
    public Transform cameraTransform;
    private Vector3 cameraFollowVelocity = Vector3.zero;
    public LayerMask collisionLayers;
    private float defaultPosition;
    private Vector3 cameraVectorPosition;

    public float cameraCollisionOffset = 0.2f; //How much does the camera jump from objects it collides with
    public float minimumCollisionOffset = 0.2f;
    public float cameraCollisionRadius = 0.2f;
    public float cameraFollowSpeed = 0.2f;
    [Header("Sensitivity")]
    public float cameraLookSpeed = 2;
    public float cameraPivotSpeed = 2;

    public float lookAngle; //Look up and down
    public float pivotAngle; //Look left and right
    public float minimumPivotAngle = -35;
    public float maximumPivotAngle = 35;

    private void Awake() {
        targetTransform = FindObjectOfType<PlayerManager>().transform;
        inputManager = FindObjectOfType<InputManager>();
        cameraTransform = Camera.main.transform;
        defaultPosition = cameraTransform.localPosition.z;
    }

    private void FollowTarget() {
        Vector3 targetPosition = Vector3.SmoothDamp(transform.position, targetTransform.position, ref cameraFollowVelocity, cameraFollowSpeed);

        transform.position = targetPosition;
    }

    private void RotateCamera() {
        Vector3 rotation;
        Quaternion targetRotation;

        lookAngle = lookAngle + (inputManager.cameraInputX * cameraLookSpeed);
        pivotAngle = pivotAngle - (inputManager.cameraInputY * cameraPivotSpeed);

        pivotAngle = Mathf.Clamp(pivotAngle, minimumPivotAngle, maximumPivotAngle);

        rotation = Vector3.zero;
        rotation.y = lookAngle;
        targetRotation = Quaternion.Euler(rotation);
        transform.rotation = targetRotation;

        rotation = Vector3.zero;
        rotation.x = pivotAngle;
        targetRotation = Quaternion.Euler(rotation);
        cameraPivot.localRotation = targetRotation;
    }

    public void HandleAllCameraMovement() {
        FollowTarget();
        RotateCamera();
        HandleCameraCollisions();
    }

    Vector3 hitPos;

    private void HandleCameraCollisions() {
        float targetPosition = defaultPosition;
        RaycastHit hit;
        Vector3 direction = cameraTransform.position - cameraPivot.position;
        direction.Normalize();

        if (Physics.SphereCast(cameraPivot.transform.position, cameraCollisionRadius, direction, out hit, Mathf.Abs(targetPosition), collisionLayers)){
            hitPos = hit.point;
            float distance = Vector3.Distance(cameraPivot.position, hit.point);
            targetPosition = -(distance-cameraCollisionOffset);
        }

        if (Mathf.Abs(targetPosition) < minimumCollisionOffset) {
            targetPosition = targetPosition - minimumCollisionOffset;
        }

        cameraVectorPosition.z = Mathf.Lerp(cameraTransform.localPosition.z, targetPosition, 0.2f);
        cameraTransform.localPosition = cameraVectorPosition;
    }
}
