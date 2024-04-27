using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proyect
{
    public class InputManager : MonoBehaviour
    {
        PlayerInputs playerInputs;

        public Vector2 movementInput;
        public float vInput;
        public float hInput;

        public bool jump = false;

        private void OnEnable() {
            if(playerInputs == null) {
                playerInputs = new PlayerInputs();

                playerInputs.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
                playerInputs.PlayerMovement.Jump.performed += i => jump = true;
            }

            playerInputs.Enable();
        }

        private void OnDisable() {
            playerInputs.Disable();
        }

        // Update is called once per frame
        void Update()
        {
            //Handle movement
            vInput = movementInput.y;
            hInput = movementInput.x;

            //Handle jumping
        }
    }
}
