using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Mine/Atmosphere", typeof(UniversalRenderPipeline))]
public class AtmospherePost : VolumeComponent, IPostProcessComponent {
    public Vector3Parameter planetCenter = new Vector3Parameter(Vector3.zero);
    public FloatParameter planetRadius = new FloatParameter(100);
    public FloatParameter atmosphereRadius = new FloatParameter(100);
    public IntParameter numInScatteringPoints = new IntParameter(5);
    public IntParameter numOutScatteringPoints = new IntParameter(5);
    public Vector3Parameter directionToSun = new Vector3Parameter(Vector3.zero);
    public FloatParameter densityFalloff = new FloatParameter(1);

    public bool IsActive() => true;
    public bool IsTileCompatible() => true;

    
}
