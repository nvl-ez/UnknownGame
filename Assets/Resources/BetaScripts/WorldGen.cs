using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

namespace Proyect
{
    public class WorldGen : MonoBehaviour
    {
        public ComputeShader computeShader;
        ComputeBuffer computeBuffer;

        float[] samples;

        // Start is called before the first frame update
        void Start(){
            int nSamples = ChunkProperties.Xsamples * ChunkProperties.Ysamples * ChunkProperties.Zsamples;

            int kernel = computeShader.FindKernel("CSMain");

            computeBuffer = new ComputeBuffer(nSamples, sizeof(float));
            computeShader.SetBuffer(kernel, "samples", computeBuffer);

            computeShader.Dispatch(kernel, ChunkProperties.Xsamples / 8, ChunkProperties.Ysamples / 8, ChunkProperties.Zsamples/8);
            samples = new float[nSamples];

            computeBuffer.GetData(samples);

            computeBuffer.Release();
        }

        private void OnDrawGizmos() {
            if(Application.isPlaying == false) return;
            for (int i = 0; i < samples.Length; i++) { 
                Gizmos.color = samples[i] > 0 ? Color.white : Color.black;
                Gizmos.DrawSphere(itopos(i), 0.1f);
                Debug.Log(itopos(i));
            }
        }

        Vector3 itopos(int i) {
            Vector3 pos;
            pos.z = (i / (ChunkProperties.Xsamples * ChunkProperties.Ysamples));
            i = i % (ChunkProperties.Xsamples * ChunkProperties.Ysamples);
            pos.y = (i / ChunkProperties.Xsamples);
            pos.x = (i % ChunkProperties.Xsamples);
            return pos;
        }

    }
}
