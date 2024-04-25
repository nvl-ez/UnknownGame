using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proyect
{
    public class MovingSphere : MonoBehaviour
    {
        [SerializeField, Range(0f, 100f)]
        float maxSpeed = 10f;
        [SerializeField, Range(0f, 100f)]
        float maxAcceleration = 10f;
        [SerializeField, Range(0f, 10f)]
        float jumpHeight = 2f;
        [SerializeField]
        int maxAirJumps = 0;

        int jumpPhase;

        Vector3 velocity;
        Vector3 desiredVelocity;

        bool onGround;

        InputHandler inputHandler;
        Rigidbody body;

        private void Awake() {
            inputHandler = GetComponent<InputHandler>();    
            body = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            desiredVelocity = new Vector3(inputHandler.hInput, 0f, inputHandler.vInput)*maxSpeed;
            
        }

        private void FixedUpdate() {
            UpdateState();
            float maxSpeedChange = maxAcceleration * Time.deltaTime;
            velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
            velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

            if (inputHandler.jump) {
                inputHandler.jump = false;
                Jump();
            }

            body.velocity = velocity;
            onGround = false;
        }

        void Jump() {
            if (onGround || jumpPhase < maxAirJumps) {
                jumpPhase += 1;
                float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);

                if(velocity.y > 0f) {
                    jumpSpeed = Mathf.Max(jumpSpeed-velocity.y, 0f);
                }

                velocity.y += jumpSpeed;
            }
        }

        private void OnCollisionEnter(Collision collision) {
            onGround = true;
        }

        private void OnCollisionStay(Collision collision) {
            onGround = true;
        }

        void EvaluateCollision(Collision collision) {
            for(int i = 0; i< collision.contactCount; i++) {
                Vector3 normal = collision.GetContact(i).normal;
                onGround |= normal.y >= 0.9f;
            }
        }

        void UpdateState() {
            velocity = body.velocity;
            if (onGround) jumpPhase = 0;
        }
    }
}
