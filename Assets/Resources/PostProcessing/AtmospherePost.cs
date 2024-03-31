using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Mine/Atmosphere", typeof(UniversalRenderPipeline))]
public class AtmospherePost : VolumeComponent, IPostProcessComponent {
    public FloatParameter tintIntensity = new FloatParameter(3);
    public ColorParameter tintColor = new ColorParameter(Color.red);

    public bool IsActive() => true;
    public bool IsTileCompatible() => true;

    
}
