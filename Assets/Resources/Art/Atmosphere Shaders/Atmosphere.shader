Shader "Mine/Atmosphere"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Float) = 0.5
        _OverlayColor ("Overlay Color", Color) = (1,0,0,1)
    }
    SubShader
    {
        //Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityPBSLighting.cginc"

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            float3 _PlanetCenter;
            float _PlanetRadius;
            float _AtmosphereRadius;
            int _NumInScatteringPoints;
            int _NumOutScatteringPoints;
            float3 _DirectionToSun;
            float _DensityFalloff;
            float3 _ScatteringCoefficients;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 viewVector : TEXCOORD1; //represents the direction from the camera to each vertex in camera space.
            };

            float3 interpolateColors(float3 col, float3 light, float min, float max, float val){
                if (min >= max) {
                    // Handle the error case, maybe return one of the colors or a default color
                    return float3(1, 0, 0); // Example: return red to indicate an error
                }

                if (val <= min) {
                    return col;
                } else if (val >= max) {
                    return light;
                }
                // Normalize val to a 0-1 range where min maps to 0 and max maps to 1
                float t = (val - min) / (max - min);
                // Interpolate between col and light based on the normalized val
                return lerp(col, light, t);
            }

            float2 raySphereIntersect(float3 rayOrg, float3 rayDir, float3 sphereCenter, float sphereRadius){
                //o: rayOrg Origin of the ray
                //d: rayDir Direction of the ray
                //Cs: sphereCenter Origin of the sphere
                //r: sphereRadius Radius of the sphere
                float3 v = rayOrg-sphereCenter; //Vector from the center of the sphere to the origin of the line
                //d^2t+2vdt+v^2-r^2
                //at^2+bt+c=0 Quadratic Formula
                float a = dot(rayDir, rayDir);
                float b = 2*dot(v, rayDir);
                float c = dot(v, v)-sphereRadius*sphereRadius;

                float discriminant = b*b-4*a*c; //If <0: no intersection | If =0: tangent | If >0: intersects in 2 points

                if(discriminant>0){
                    float root = sqrt(discriminant);
                    float nearT = max(0, (-b-root)/(2*a));
                    float farT = (-b+root)/(2*a);

                    //From sebastian lague: Ignore intersections that occur behind the rayOrigin
                    if(farT>=0){
                        return float2(nearT, farT-nearT);
                    }
                }

                return float2(-1, 0);
            }

            //Calculates the density at a point in the atmosphere
            float densityAtPoint(float3 pointSample){
                float heightAboveSurface = length(pointSample-_PlanetCenter)-_PlanetRadius;
                float height01 = heightAboveSurface/(_AtmosphereRadius-_PlanetRadius);
                float localDensity = exp(-height01*_DensityFalloff)*(1-height01);
                return localDensity;
            }

            //Does the same as the calculateLight function but from the surface of the atmosphere to a inScatterPoint
            float opticalDepth(float3 rayOrg, float3 rayDir, float3 rayLength){
                float3 densitySamplePoint = rayOrg;
                float stepSize = rayLength/(_NumOutScatteringPoints-1);
                float opticalDepth = 0;

                for(int i = 0; i<_NumOutScatteringPoints; i++){
                    float localDensity = densityAtPoint(densitySamplePoint);
                    opticalDepth += localDensity*stepSize;
                    densitySamplePoint += rayDir*stepSize;
                }
                return opticalDepth;
            }

            //Returns all the light that makes it to the camera 
            float3 calculateLight(float3 rayOrg, float3 rayDir, float rayLength, float3 col){
                float3 inScatterPoint = rayOrg;
                float stepSize = rayLength/(_NumInScatteringPoints-1);
                float3 inScatteredLight = 0;
                float viewRayOpticalDepth = 0;

                for(int i = 0; i<_NumInScatteringPoints; i++){
                    //Get the distance of the sun ray from the sun to the inScattering point evaluating rn
                    float sunRayLength = raySphereIntersect(inScatterPoint, _DirectionToSun, _PlanetCenter, _AtmosphereRadius).y;
                    //Light might be lost from the sun ray (inside of the atmosphere) to the inScatter point
                    float sunRayOpticalDepth = opticalDepth(inScatterPoint, _DirectionToSun, sunRayLength);
                    //Light might be lost from the inScatter point to the camera
                    viewRayOpticalDepth = opticalDepth(inScatterPoint, -rayDir, stepSize*i);
                    //Transmittance is proportion of light that makes it to the inScatter point : As the optical depth increases, transmittance decreases exponentially
                    float3 transmittance = exp(-(sunRayOpticalDepth+viewRayOpticalDepth)*_ScatteringCoefficients);

                    float localDensity = densityAtPoint(inScatterPoint);

                    inScatteredLight += localDensity*transmittance*_ScatteringCoefficients*stepSize;
                    inScatterPoint += rayDir*stepSize;
                }
                float originalColTransmittance = exp(-viewRayOpticalDepth);
                return col *originalColTransmittance + inScatteredLight;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                /*
                "v.uv.xy * 2 - 1": Changes range from 0..1 to -1..1 to reverse-engineer the position of the vertex in camera space using its UV coordinates, and in clip space.

                "mul(unity_CameraInvProjection, ...)": applies the inverse of the camera's projection matrix to the adjusted UV coordinates. 
                    The inverse projection matrix transforms coordinates from projection space (after the projection transformation has been applied) back to camera space. 
                    This step is trying to get the direction vectors in camera space that point from the camera to where the vertices would be before they were projected.
                */
                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv.xy * 2 - 1, 0, -1));
                /*
                "unity_CameraToWorld": transforms coordinates from camera space to world space.
                */
				o.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float sceneDepthNonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float sceneDepth = LinearEyeDepth(sceneDepthNonLinear)*length(i.viewVector);
                
                float3 rayOrg = _WorldSpaceCameraPos;
                float3 rayDir = normalize(i.viewVector);

                float2 hit = raySphereIntersect(rayOrg, rayDir, _PlanetCenter, _AtmosphereRadius);
                float dstToAtmosphere = hit.x;
                float dstThroughAtmosphere = min(hit.y, sceneDepth-dstToAtmosphere);

                if(dstThroughAtmosphere>0){
                    float3 pointInAtmosphere = rayOrg+rayDir*dstToAtmosphere;
                    float3 light = calculateLight(pointInAtmosphere, rayDir, dstThroughAtmosphere, col);
                    float3 colors = interpolateColors(col, light, 10, 20, dstThroughAtmosphere);
                    return float4(colors, 0);
                }

                return col;
            }
            ENDCG
        }
    }
}
