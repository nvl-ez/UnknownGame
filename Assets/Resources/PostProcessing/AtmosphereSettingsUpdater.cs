using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AtmosphereSettingsUpdater : MonoBehaviour
{
    [Header("Debug Parameters (dont modify)")]
    public AtmospherePost atmospherePost;
    public WorldGenerator worldGenerator;
    public GameObject sun;
    public ComputeShader opticalDepthCompute;
    public RenderTexture opticalDepthTexture;

    [Header("Optical Depth Texture")]
    public int textureSize = 256;
    public int opticalDepthPoints = 10;

    void Awake() {
        //Idk why cannot be done simpler but works UwU
        Volume volume = gameObject.GetComponent<Volume>();
        AtmospherePost tmp;
        if (volume.profile.TryGet<AtmospherePost>(out tmp)) {
            atmospherePost = tmp;
        }
        worldGenerator = GameObject.Find("World Generator").GetComponent<WorldGenerator>();
        sun = GameObject.Find("Sun");
        opticalDepthCompute = Resources.Load<ComputeShader>("Scripts/Atmosphere Compute/AtmosphereTexture");

        UpdateAtmosphereSettings(false);
    }

    private void Update() {
        UpdateAtmosphereSettings(true);
    }

    void UpdateAtmosphereSettings(bool updating) {
        if (atmospherePost != null && worldGenerator != null) {
            // Set the world Position
            atmospherePost.planetCenter.Override(worldGenerator.transform.position);

            //Set the sun position
            atmospherePost.directionToSun.Override((sun.transform.position - worldGenerator.transform.position).normalized);

            if (!updating) {
                //Set the world radius
                atmospherePost.planetRadius.Override(1);

                //Set the atmosphere radius;
                atmospherePost.atmosphereRadius.Override(worldGenerator.planetRadius);

                //set inScattering points
                atmospherePost.numInScatteringPoints.Override(5);

                //set density falloff points
                atmospherePost.densityFalloff.Override(5);

                //set the wavelengths
                atmospherePost.wavelengths.Override(new Vector3(700, 530, 440));

                //set the scattering Strength
                atmospherePost.scatteringStrength.Override(1);

                //generate the optical depth texture
                precomputeOpticalDepthTexture();
                atmospherePost.bakedOpticalDepth.Override(opticalDepthTexture);
            }
            
        }
    }

    //All the code below comes from sebastian lague's github. I could have implemented it bc it is just
    //copying the functions from the atmosphere shader, but it is o much easier if it is already done and works ;-;
    void precomputeOpticalDepthTexture() {
        CreateRenderTexture(ref opticalDepthTexture, textureSize, FilterMode.Bilinear);

        opticalDepthCompute.SetTexture(0, "Result", opticalDepthTexture);
        opticalDepthCompute.SetInt("textureSize", textureSize);
        opticalDepthCompute.SetInt("numOutScatteringSteps", opticalDepthPoints);
        opticalDepthCompute.SetFloat("atmosphereRadius", atmospherePost.atmosphereRadius.value);
        opticalDepthCompute.SetFloat("densityFalloff", atmospherePost.densityFalloff.value);

        Run(opticalDepthCompute, textureSize, textureSize);
    }

    public static void CreateRenderTexture(ref RenderTexture texture, int size, FilterMode filterMode = FilterMode.Bilinear, GraphicsFormat format = GraphicsFormat.R16G16B16A16_SFloat) {
        CreateRenderTexture(ref texture, size, size, filterMode, format);
    }

    public static void CreateRenderTexture(ref RenderTexture texture, int width, int height, FilterMode filterMode = FilterMode.Bilinear, GraphicsFormat format = GraphicsFormat.R16G16B16A16_SFloat) {
        if (texture == null || !texture.IsCreated() || texture.width != width || texture.height != height || texture.graphicsFormat != format) {
            if (texture != null) {
                texture.Release();
            }
            texture = new RenderTexture(width, height, 0);
            texture.graphicsFormat = format;
            texture.enableRandomWrite = true;

            texture.autoGenerateMips = false;
            texture.Create();
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = filterMode;
    }

    public static void Run(ComputeShader cs, int numIterationsX, int numIterationsY = 1, int numIterationsZ = 1, int kernelIndex = 0) {
        Vector3Int threadGroupSizes = GetThreadGroupSizes(cs, kernelIndex);
        int numGroupsX = Mathf.CeilToInt(numIterationsX / (float)threadGroupSizes.x);
        int numGroupsY = Mathf.CeilToInt(numIterationsY / (float)threadGroupSizes.y);
        int numGroupsZ = Mathf.CeilToInt(numIterationsZ / (float)threadGroupSizes.y);
        cs.Dispatch(kernelIndex, numGroupsX, numGroupsY, numGroupsZ);
    }

    public static Vector3Int GetThreadGroupSizes(ComputeShader compute, int kernelIndex = 0) {
        uint x, y, z;
        compute.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        return new Vector3Int((int)x, (int)y, (int)z);
    }
}
