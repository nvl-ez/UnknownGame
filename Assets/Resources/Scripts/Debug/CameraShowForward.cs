using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShowForward : MonoBehaviour
{
    bool showRight = true;
    private void OnDrawGizmos() {
        Camera camera = Camera.main;

        Vector3 start = camera.transform.position;

        Vector3 end = start + camera.transform.forward * 5;

        // Draw the line
        Gizmos.color = Color.red;
        Gizmos.DrawLine(start, end);

        if(showRight) {
            end = start + camera.transform.right * 5;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(start, end);
        }
    }
}
