using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WorldGenerator : MonoBehaviour
{
    public int chunkRadius;
    GameObject world;


    // Start is called before the first frame update
    void Awake()
    {
        generate();
    }

    // Update is called once per frame
    void setUpNoiseGenerator(float radius)
    {
        NoiseGenerator noiseGenerator = GetComponent<NoiseGenerator>();
        
        noiseGenerator.radius = radius;
        radius -= noiseGenerator.ground;
        noiseGenerator.insideRadius = radius*0.40f;
        noiseGenerator.ground = radius * 0.10f;
        noiseGenerator.air = radius * 0.05f;
    }

    GameObject generateCore(float radius) {
        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.name = "Core";
        core.tag = "Terrain";
        core.layer = LayerMask.NameToLayer("Terrain");
        core.transform.position = Vector3.zero;
        core.transform.localScale = Vector3.one*0.3f*radius;
        core.transform.parent = world.transform;
        return core;
    }

    void generate() {
        //Parent object for all chunks and responsible for Gravity
        world = new GameObject("World");
        world.AddComponent<GravityAttractor>();
        world.transform.tag = "Planet";
        world.transform.position = Vector3.zero;

        float radius = Random.Range((chunkRadius - 1.0f) * GridMetrics.Scale, chunkRadius * GridMetrics.Scale);

        //Create the core of the world
        GameObject core = generateCore(radius);

        //Set the radius of the world in UNITS

        setUpNoiseGenerator(radius);

        chunkRadius += 1; //So terrain generated on the edge doesnt cut suddenly

        //Generate 3D array of chunks to keep trak of them and generate all chunks
        //worldChunks = new GameObject[(chunkRadius * 2) * (chunkRadius * 2) * (chunkRadius * 2)];

        for (int chunkX = -chunkRadius; chunkX < chunkRadius; chunkX++) {
            for (int chunkY = -chunkRadius; chunkY < chunkRadius; chunkY++) {
                for (int chunkZ = -chunkRadius; chunkZ < chunkRadius; chunkZ++) {
                    // Instantiate the chunk
                    GameObject prefabChunk = Instantiate(Resources.Load<GameObject>("Prefabs/Chunk"), new Vector3(chunkX, chunkY, chunkZ) * GridMetrics.Scale, Quaternion.identity);
                    prefabChunk.transform.parent = world.transform;
                }
            }
        }
    }
}
