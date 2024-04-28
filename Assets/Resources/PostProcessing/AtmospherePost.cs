using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Mine/Atmosphere", typeof(UniversalRenderPipeline))]
public class AtmospherePost : VolumeComponent, IPostProcessComponent {
    public BoolParameter activeState = new BoolParameter(true);
    public Vector3Parameter planetCenter = new Vector3Parameter(Vector3.zero);
    public FloatParameter planetRadius = new FloatParameter(100);
    public FloatParameter atmosphereRadius = new FloatParameter(100);
    public IntParameter numInScatteringPoints = new IntParameter(5);
    public Vector3Parameter directionToSun = new Vector3Parameter(Vector3.zero);
    public FloatParameter densityFalloff = new FloatParameter(1);
    public Vector3Parameter wavelengths = new Vector3Parameter(Vector3.zero);
    public FloatParameter scatteringStrength = new FloatParameter(1);
    public TextureParameter bakedOpticalDepth = new TextureParameter(null);

    public bool IsActive() {
        return activeState.value;
    }
    public bool IsTileCompatible() => true;
}
