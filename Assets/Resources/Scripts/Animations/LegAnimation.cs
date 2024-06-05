using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Proyect
{
    public class LegAnimation : MonoBehaviour
    {
        public bool domove = true;
        //Movement
        public Vector3 center;
        public float radius;
        public float speed;
        public Vector3 targetDirection;
        public float angle;

        public float smoothness = 1f;
        public float stepHeight = 0.1f;
        public Transform[] legs;
        public Transform[] targets;
        Vector3[] oldpos = new Vector3[6];
        bool[] iterating = new bool[6];
        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0; i < legs.Length; i++) {
                oldpos[i] = legs[i].position;
                iterating[i] = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(domove) move();

            int index = getFurtherLeg();

            if(index != -1) {
                Vector3 end = targets[index].position+targetDirection.normalized*0.29f;
                iterating[index] = true;
                StartCoroutine(performStep(index, legs[index].position, end));
            }

        }

        int getFurtherLeg() {
            float max = 0f;
            int index = -1;
            for(int i = 0; i < legs.Length; i++) {
                float distance = Vector3.Distance(targets[i].position, oldpos[i]);
                if (distance > max) {
                    max = distance;
                    index = i;
                }

                if (!iterating[i]) legs[i].position = oldpos[i];
            }
            if(max < 0.30f)index = -1;
            return index;
        }

        IEnumerator performStep(int index, Vector3 origin, Vector3 end) {
            for (int i = 1; i <= smoothness; ++i) {
                legs[index].position = Vector3.Lerp(origin, end, i / (float)(smoothness + 1f));
                legs[index].position += transform.up * Mathf.Sin(i / (float)(smoothness + 1f) * Mathf.PI) * stepHeight;
                yield return new WaitForFixedUpdate();
            }
            legs[index].position = end;
            oldpos[index] = end;
            iterating[index] = false;
        }

        void move() {
            angle += speed * Time.deltaTime; // Increment the angle based on the speed and time

            float x = Mathf.Cos(angle) * radius; // Calculate the x position
            //float y = Mathf.Sin(angle) * radius; // Calculate the y position (for 2D)
            float z = Mathf.Sin(angle) * radius; // Uncomment this for 3D circular motion

            targetDirection = transform.position;
            
            transform.position = new Vector3(center.x + x, center.y, center.z + z);

            targetDirection = -transform.position+targetDirection;

            transform.rotation = Quaternion.LookRotation(targetDirection);

            targetDirection = -targetDirection;
        }
    }
}
