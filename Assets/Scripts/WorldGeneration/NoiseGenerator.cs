using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class NoiseGenerator : MonoBehaviour
{
    [Header("Caves Properties")]
    [SerializeField] float caveAmplitude = 5f;
    [SerializeField] float caveFrequency = 0.005f;
    [SerializeField] int caveOctaves = 8;
    [Header("\nGround Properties")]
    [SerializeField] float groundAmplitude = 5f;
    [SerializeField] float groundFrequency = 0.005f;
    [SerializeField] int groundOctaves = 8;
    [Header("\nIslands Properties")]
    [SerializeField] float islandsAmplitude = 5f;
    [SerializeField] float islandsFrequency = 0.005f;
    [SerializeField] int islandsOctaves = 8;

    [Header("\nGenerated Values (Dont modify)")]
    public float radius;
    public float insideRadius;
    public float crustWidth = 3;
    public float ground;
    public float air;
    
    private int seed;

    ComputeBuffer _weightsBuffer;
    public ComputeShader NoiseShader;

    private void Awake() {
        seed = Random.Range(-10, 10);
    }

    //Creates a buffer for all the noise values for the points in the gpu
    void CreateBuffers(int lod) {
        _weightsBuffer = new ComputeBuffer(
            GridMetrics.PointsPerChunk(lod) * GridMetrics.PointsPerChunk(lod) * GridMetrics.PointsPerChunk(lod), sizeof(float)
        );
    }

    //Releases the memory used
    void ReleaseBuffers() {
        _weightsBuffer.Release();
    }

    //Executes the noise calculations on the GPU and obtains its values
    public float[] GetNoise(int lod, Vector3 offset) {
        CreateBuffers(lod);

        float[] noiseValues = new float[GridMetrics.PointsPerChunk(lod) * GridMetrics.PointsPerChunk(lod) * GridMetrics.PointsPerChunk(lod)];

        //Asignt the buffer
        NoiseShader.SetBuffer(0, "_Weights", _weightsBuffer);

        //Sets all the parameters for calculating the noise
        NoiseShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk(lod));
        NoiseShader.SetVector("_Offset", offset);

        NoiseShader.SetFloat("_caveAmplitude", caveAmplitude);
        NoiseShader.SetFloat("_caveFrequency", caveFrequency);
        NoiseShader.SetInt("_caveOctaves", caveOctaves);

        NoiseShader.SetFloat("_groundAmplitude", groundAmplitude);
        NoiseShader.SetFloat("_groundFrequency", groundFrequency);
        NoiseShader.SetInt("_groundOctaves", groundOctaves);

        NoiseShader.SetFloat("_islandsAmplitude", islandsAmplitude);
        NoiseShader.SetFloat("_islandsFrequency", islandsFrequency);
        NoiseShader.SetInt("_islandsOctaves", islandsOctaves);

        NoiseShader.SetFloat("_Radius", radius);
        NoiseShader.SetFloat("_insideRadius", insideRadius);
        NoiseShader.SetFloat("_crustWidth", crustWidth);
        NoiseShader.SetFloat("_ground", ground);
        NoiseShader.SetFloat("_air", air);

        NoiseShader.SetInt("_Seed", seed);
        NoiseShader.SetInt("_Scale", GridMetrics.Scale);

        //Start the shader
        NoiseShader.Dispatch(0, 
            GridMetrics.ThreadGroups(lod),
            GridMetrics.ThreadGroups(lod),
            GridMetrics.ThreadGroups(lod));

        //Retrieve the data
        _weightsBuffer.GetData(noiseValues);

        ReleaseBuffers();

        return noiseValues;
    }
}
