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
        public float moveAmount;

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

        private void Awake() {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
                // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            //Handle movement
            vInput = movementInput.y;
            hInput = movementInput.x;

            moveAmount = Mathf.Clamp01(Mathf.Abs(hInput)+Mathf.Abs(vInput));
        }
    }
}
