using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerraformingCamera : MonoBehaviour
{
    Camera _cam;
    public float BrushSize = 2f;
    public WorldGenerator worldGenerator;

    private void Awake() {
        _cam = GetComponent<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    /*private void LateUpdate() {
        if (Input.GetMouseButton(0)) {
            Terraform(true);
        } else if (Input.GetMouseButton(1)) {
            Terraform(false);
        }
    }

    //Shoots a ray and returns the chunk that collided with, bool add = true: add terain | add = false: remove terrain
    private void Terraform(bool add) {
        RaycastHit hit;

        if (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out hit, 1000)) {
            Vector3 _hitPoint = hit.point;

            //Array position of the chunk collided
            int coordX = (int)hit.collider.gameObject.transform.position.x / GridMetrics.Scale + worldGenerator.chunkRadius;
            int coordY = (int)hit.collider.gameObject.transform.position.y / GridMetrics.Scale + worldGenerator.chunkRadius;
            int coordZ = (int)hit.collider.gameObject.transform.position.z / GridMetrics.Scale + worldGenerator.chunkRadius;
            //Obtain how many chunks around the chunk collided need to be edited
            int chunkOffset = (int)(BrushSize + GridMetrics.EdgeDistance) / GridMetrics.Scale + 1;

            //Go through all chunks and modify the weights. NEW ALGORITHM NEEDED
            for (int x = coordX - chunkOffset; x <= coordX + chunkOffset; x++) {
                for (int y = coordY - chunkOffset; y <= coordY + chunkOffset; y++) {
                    for (int z = coordZ - chunkOffset; z <= coordZ + chunkOffset; z++) {
                        if (x < 0 || y < 0 || z < 0 || x >= worldGenerator.chunkRadius * 2 || y >= worldGenerator.chunkRadius * 2 || z >= worldGenerator.chunkRadius * 2) continue;
                        GameObject chunk = worldGenerator.worldChunks[GridMetrics.chunk3Dto1D(x, y, z, worldGenerator.chunkRadius)];
                        Vector3 localHitPoint = chunk.transform.InverseTransformPoint(hit.point);
                        chunk.GetComponent<Chunk>().EditWeights(localHitPoint, BrushSize, add);
                    }
                }
            }
        }
    }
    */
}
