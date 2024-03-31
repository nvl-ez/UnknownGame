using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AtmosphereSettingsUpdater : MonoBehaviour
{
    public AtmospherePost atmospherePost;
    public WorldGenerator worldGenerator;
    public GameObject sun;

    void Awake() {
        //Idk why cannot be done simpler but works UwU
        Volume volume = gameObject.GetComponent<Volume>();
        AtmospherePost tmp;
        if (volume.profile.TryGet<AtmospherePost>(out tmp)) {
            atmospherePost = tmp;
        }
        worldGenerator = GameObject.Find("World Generator").GetComponent<WorldGenerator>();
        sun = GameObject.Find("Sun");

        UpdateAtmosphereSettings(false);
    }

    private void Update() {
        UpdateAtmosphereSettings(true);
    }

    void UpdateAtmosphereSettings(bool updating) {
        if (atmospherePost != null && worldGenerator != null) {
            // Set the world Position
            atmospherePost.planetCenter= new Vector3Parameter(worldGenerator.transform.position);

            //Set the sun position
            atmospherePost.directionToSun = new Vector3Parameter((sun.transform.position - worldGenerator.transform.position).normalized);

            if (!updating) {
                //Set the world radius
                atmospherePost.planetRadius.Override(worldGenerator.insideRadius + worldGenerator.ground/2);

                //set inScattering points
                atmospherePost.numInScatteringPoints.Override(5);

                //set outScattering points
                atmospherePost.numOutScatteringPoints.Override(5);

                //set density falloff points
                atmospherePost.densityFalloff.Override(5);
            }
            
        }
    }

}
