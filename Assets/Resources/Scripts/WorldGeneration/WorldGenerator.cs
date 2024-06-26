using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class WorldGenerator : MonoBehaviour
{
    public int chunkRadius;
    [Header("World Properties")]
    public float cavePercentage = 0.4f;
    public float groundPercentage = 0.1f;
    public float airPercentage = 0.05f; //Floating island be the remaining percentage

    [Header("World Stats")]
    public float planetRadius;
    public float insideRadius;
    public float ground;
    public float air;

    [Header("Atmosphere Properties")]
    public Material mat;
    public Color skyColor;

    [Header("Trees")]
    public Material trunkMaterial;
    public Material leavesMaterial;
    public Mesh leavesMesh;

    GameObject world;


    // Start is called before the first frame update
    void Awake()
    {
        //Generate world
        generate();

        //Setup skybox
        setUpSkybox();

    }

    private void Update() {
        
    }

    //setup skybox
    void setUpSkybox() {
        mat = new Material(Resources.Load<Shader>("Art/Atmosphere Shaders/SkyboxShader"));
        mat.SetColor("_SkyColor", skyColor);
        mat.SetVector("_PlanetCenter", world.transform.position);
        mat.SetFloat("_AtmosphereRadius", planetRadius);
        mat.SetFloat("_PlanetRadius", insideRadius+ground+air);
    }

    // Update is called once per frame
    void setUpNoiseGenerator()
    {
        NoiseGenerator noiseGenerator = GetComponent<NoiseGenerator>();
        planetRadius = Random.Range((chunkRadius - 1.0f) * GridMetrics.Scale, chunkRadius * GridMetrics.Scale);

        noiseGenerator.radius = planetRadius;
        planetRadius -= noiseGenerator.ground;
        insideRadius = planetRadius*0.40f;
        noiseGenerator.insideRadius = insideRadius;
        ground = planetRadius * 0.10f;
        noiseGenerator.ground = ground;
        air = planetRadius * 0.05f;
        noiseGenerator.air = air;
    }

    Material initWorldMaterial(Vector3 worldPos) {
        Material material =new Material( Resources.Load<Shader>("Art/Terrain Shader/Planet Shader"));
        material.SetVector("_World_Center", worldPos);
        material.SetFloat("_Inside_Radius_Min", insideRadius+ground/2);
        material.SetFloat("_Inside_Radius_Max", insideRadius+ground/2);
        return material;
    }

    GameObject generateCore() {
        Material material = new Material(Resources.Load<Shader>("Art/Terrain Shader/Core"));
        material.SetFloat("_size", 10f);
        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.GetComponent<MeshRenderer>().material = material;
        core.name = "Core";
        core.tag = "Terrain";
        core.transform.position = Vector3.zero;
        core.transform.localScale = Vector3.one*0.3f*planetRadius;
        core.transform.parent = world.transform;
        return core;
    }

    void generate() {
        

        //Parent object for all chunks and responsible for Gravity
        world = new GameObject("World");

        world.transform.position = Vector3.zero;
        transform.position = Vector3.zero;

        //Create the core of the world

        setUpNoiseGenerator();
        GameObject core = generateCore();

        chunkRadius += 1; //So terrain generated on the edge doesnt cut suddenly

        Material material = initWorldMaterial(world.transform.position);

        //Generate 3D array of chunks to keep trak of them and generate all chunks
        //worldChunks = new GameObject[(chunkRadius * 2) * (chunkRadius * 2) * (chunkRadius * 2)];

        for (int chunkX = -chunkRadius; chunkX < chunkRadius; chunkX++) {
            for (int chunkY = -chunkRadius; chunkY < chunkRadius; chunkY++) {
                for (int chunkZ = -chunkRadius; chunkZ < chunkRadius; chunkZ++) {
                    // Instantiate the chunk
                    GameObject prefabChunk = Instantiate(Resources.Load<GameObject>("Prefabs/Chunk"), new Vector3(chunkX, chunkY, chunkZ) * GridMetrics.Scale, Quaternion.identity);
                    prefabChunk.transform.parent = world.transform;
                    prefabChunk.GetComponent<Renderer>().material = material;
                }
            }
        }

        addTrees();
    }

    void addTrees() {
        TreeGenerator generator = new TreeGenerator();
        generator.leavesMesh = leavesMesh;
        generator.leavesMaterial = leavesMaterial;
        generator.trunkMaterial = trunkMaterial;

        for (int i = 0; i < 90; i++) {
            Vector3 origin = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * planetRadius;

            RaycastHit hit;
            if (Physics.Raycast(origin, -origin, out hit, planetRadius, LayerMask.NameToLayer("Terrain"))) {
                generator.placeTree(hit.point, hit.normal);
            }
        }
    }
}
