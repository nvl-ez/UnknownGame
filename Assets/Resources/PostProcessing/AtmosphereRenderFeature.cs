using System;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AtmosphereRenderFeature : ScriptableRendererFeature {
    private AtmospherePass atmospherePass;


    public override void Create() {
        atmospherePass = new AtmospherePass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass(atmospherePass);
    }

    class AtmospherePass : ScriptableRenderPass {
        static Vector3 scatteringCoefficients = Vector3.zero;
        private Material _mat;
        int tintId = Shader.PropertyToID("_Temp");
        RenderTargetIdentifier src, tint;

        public AtmospherePass() {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

            // Create material only if it hasn't been created yet
            if (_mat == null) _mat = CoreUtils.CreateEngineMaterial("Mine/Atmosphere");
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            src = renderingData.cameraData.renderer.cameraColorTarget;

            RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
            cmd.GetTemporaryRT(tintId, desc, FilterMode.Bilinear);

            // Initialize RenderTargetIdentifier with the temporary render texture ID
            tint = new RenderTargetIdentifier(tintId);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            CommandBuffer commandBuffer = CommandBufferPool.Get("AtmosphereRenderFeature");

            VolumeStack volumes = VolumeManager.instance.stack;
            AtmospherePost atmospherePost = volumes.GetComponent<AtmospherePost>();


            if (!atmospherePost.IsActive()) {
                CommandBufferPool.Release(commandBuffer);
                return;
            }
   

            if (atmospherePost != null && atmospherePost.IsActive()) {
                //Calculate the coeffitients
                scatteringCoefficients.x = Mathf.Pow(400 / atmospherePost.wavelengths.value.x, 4) * atmospherePost.scatteringStrength.value;
                scatteringCoefficients.y = Mathf.Pow(400 / atmospherePost.wavelengths.value.y, 4) * atmospherePost.scatteringStrength.value;
                scatteringCoefficients.z = Mathf.Pow(400 / atmospherePost.wavelengths.value.z, 4) * atmospherePost.scatteringStrength.value;

                _mat.SetVector("_PlanetCenter", atmospherePost.planetCenter.value);
                _mat.SetFloat("_PlanetRadius", atmospherePost.planetRadius.value);
                _mat.SetFloat("_AtmosphereRadius", atmospherePost.atmosphereRadius.value);
                _mat.SetInt("_NumInScatteringPoints", atmospherePost.numInScatteringPoints.value);
                _mat.SetVector("_DirectionToSun", atmospherePost.directionToSun.value);
                _mat.SetFloat("_DensityFalloff", atmospherePost.densityFalloff.value);
                _mat.SetVector("_ScatteringCoefficients", scatteringCoefficients);
                _mat.SetTexture("_BakedOpticalDepth", atmospherePost.bakedOpticalDepth.value);
                


                Blit(commandBuffer, src, tint, _mat, 0);
                Blit(commandBuffer, tint, src);
            }

            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) {
            if (cmd != null) {
                cmd.ReleaseTemporaryRT(tintId);
            }
        }
    }
}