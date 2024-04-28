using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

namespace Proyect
{
    public class MovingSphere : NetworkBehaviour
    {
        [SerializeField, Range(0f, 100f)]
        float maxSpeed = 10f;
        [SerializeField, Range(0f, 100f)]
        float maxAcceleration = 10f, maxAirAcceleration = 1f;
        [SerializeField, Range(0f, 90f)]
        float maxGroundAngle = 25f, maxStairsAngle = 50f;
        [SerializeField, Range(0f, 10f)]
        float jumpHeight = 2f;
        [SerializeField]
        int maxAirJumps = 0;
        [SerializeField, Range(0f, 100f)]
        float maxSnapSpeed = 100f;
        [SerializeField, Min(0f)]
        float probeDistance = 1f;
        [SerializeField]
        Transform playerInputSpace = default;

        int jumpPhase;

        float minGroundDotProduct, minStairsDotProduct;

        Vector3 velocity;
        Vector3 desiredVelocity;
        Vector3 contactNormal, steepNormal;
        Vector3 upAxis, rightAxis, forwardAxis;

        [SerializeField]
        LayerMask probeMask = -1, stairsMask = -1;

        int groundContactCount, steepContactCount;
        bool OnGround => groundContactCount>0;
        bool OnSteep => steepContactCount>0;

        int stepsSinceLastGrounded, stepsSinceLastJump;

        InputManager inputHandler;
        Rigidbody body;

        private void Awake() {
            inputHandler = GetComponent<InputManager>();
            body = GetComponent<Rigidbody>();

            playerInputSpace = Camera.main.transform;

            body.useGravity = false;
            OnValidate();
        }

        // Update is called once per frame
        [Client(RequireOwnership = true)]
        void Update()
        {

            if (playerInputSpace) {
                rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
                forwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
            } else {
                rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
                forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
            }
            desiredVelocity =  new Vector3(inputHandler.hInput, 0f, inputHandler.vInput) * maxSpeed;
        }

        [Client(RequireOwnership = true)]
        private void FixedUpdate() {

            Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);
            UpdateState();
            AdjustVelocity();

            if (inputHandler.jump) {
                inputHandler.jump = false;
                Jump(gravity);
            }

            velocity += gravity * Time.deltaTime;

            body.velocity = velocity;
            ClearState();
        }

        void ClearState() {
            groundContactCount = steepContactCount = 0;
            contactNormal = steepNormal = Vector3.zero;
        }

        void Jump(Vector3 gravity) {
            Vector3 jumpDirection;

            if (OnGround) jumpDirection = contactNormal;
            else if (OnSteep) { 
                jumpDirection = steepNormal;
                jumpPhase = 0;
            } else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps) {
                if (jumpPhase == 0) jumpPhase = 1;
                jumpDirection = contactNormal;
            } else return;

            stepsSinceLastJump = 0;
            jumpPhase += 1;
            float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
            jumpDirection = (jumpDirection + upAxis).normalized;
            float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
            if(alignedSpeed > 0f) {
                jumpSpeed = Mathf.Max(jumpSpeed-alignedSpeed, 0f);
            }

            velocity += jumpDirection*jumpSpeed;
            
        }

        private void OnCollisionEnter(Collision collision) {
            EvaluateCollision(collision);
        }

        private void OnCollisionStay(Collision collision) {
            EvaluateCollision(collision);
        }

        void EvaluateCollision(Collision collision) {
            float minDot = GetMinDot(collision.gameObject.layer);
            for(int i = 0; i< collision.contactCount; i++) {
                Vector3 normal = collision.GetContact(i).normal;
                float upDot = Vector3.Dot(upAxis, normal);
                if(upDot >= minDot) {
                    groundContactCount += 1;
                    contactNormal += normal;
                } else if(upDot < -0.01f) { 
                    steepContactCount += 1;
                    steepNormal += normal;
                }
            }
        }

        void UpdateState() {
            stepsSinceLastGrounded++;
            stepsSinceLastJump++;
            velocity = body.velocity;
            AlignRotationWithGravityAndCamera();
            if (OnGround || SnapToGround() || CheckSteepContacts()) {
                stepsSinceLastGrounded = 0;

                if(stepsSinceLastJump > 1) jumpPhase = 0;

                if (groundContactCount > 0) {
                    contactNormal.Normalize();
                }
            } else {
                contactNormal = upAxis;
            }
        }

        void OnValidate() {
            minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
            minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
        }

        void AdjustVelocity() {
            Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
            Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);

            float currentX = Vector3.Dot(velocity, xAxis);
            float currentZ = Vector3.Dot(velocity, zAxis);

            float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
            float maxSpeedChange = acceleration * Time.deltaTime;

            float newX =
                Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
            float newZ =
                Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

            velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        }

        //Skips the normalization of ProjectOnPlane
        Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal) {
            return (direction - normal * Vector3.Dot(direction, normal)).normalized;
        }

        bool SnapToGround() {
            if(stepsSinceLastGrounded>1 || stepsSinceLastJump <= 2) return false;

            float speed = velocity.magnitude;
            if (speed > maxSnapSpeed) return false;

            if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit, probeDistance, probeMask)) return false;

            float upDot = Vector3.Dot(upAxis, hit.normal);
            if (upDot < GetMinDot(hit.collider.gameObject.layer)) return false;

            groundContactCount = 1;
            contactNormal = hit.normal;
            float dot = Vector3.Dot(velocity, hit.normal);
            if(dot >0f) velocity = (velocity - hit.normal * dot).normalized * speed;

            return true;
        }

        float GetMinDot(int layer) {
            return (stairsMask & (1 << layer)) == 0 ? minGroundDotProduct : minStairsDotProduct;
        }

        bool CheckSteepContacts() {
            if(steepContactCount > 1) {
                steepNormal.Normalize();
                float upDot = Vector3.Dot(upAxis, steepNormal);
                if(upDot >= minGroundDotProduct) {
                    groundContactCount = 1;
                    contactNormal = steepNormal;
                    return true;
                }
            }
            return false;
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, forwardAxis);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, rightAxis);
        }

        void AlignRotationWithGravityAndCamera() {
            Quaternion alignToGravity = Quaternion.FromToRotation(transform.up, upAxis);

            Vector3 horizontalVelocity = velocity - Vector3.Dot(velocity, upAxis) * upAxis;

            if(inputHandler.moveAmount > 0.01f && OnGround) {
                Quaternion targetRotation = Quaternion.LookRotation(forwardAxis, upAxis);
                targetRotation = alignToGravity *targetRotation;
                body.rotation = Quaternion.Slerp(body.rotation, targetRotation, 20 * Time.deltaTime);
            } else {
                Quaternion targetRotation = alignToGravity * transform.rotation;
                body.rotation = Quaternion.Lerp(body.rotation, targetRotation, 20f*Time.deltaTime);
            }
        }
    }
}
