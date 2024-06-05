using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proyect
{
    public class AnimatorController : MonoBehaviour
    {
        Animator animator;
        InputManager inputManager;

        private void Awake() {
            animator = GetComponent<Animator>();
            inputManager = GetComponent<InputManager>();

        }
        
        
        void Update()
        {
            Debug.Log(inputManager.moveAmount);
            if (inputManager.moveAmount > 0.1f) {
                animator.Play("walk");
            } else {
                animator.Play("stand");
            }
        }
    }
}
