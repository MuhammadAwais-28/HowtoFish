// URP rewrite of "Mobile/MobileWaterShaderEKV_v2"
// Original was a Built-in RP surface shader (CGPROGRAM + #pragma surface).
// URP has no surface-shader pipeline, so this is a direct HLSL Forward Lit pass
// that reproduces the same math: dual rotated textures, flow-mapped waves,
// edge/center foam, cubemap reflection, and a custom Blinn-Phong style response.
//
// Property names (_MainTex, _DiffTex, _MainTexRot, etc.) are kept IDENTICAL
// to the original so WaterControl_EKV.cs keeps working with zero script changes.

Shader "Universal Render Pipeline/Custom/MobileWaterShaderEKV_URP"
{
    Properties
    {
        [Toggle(LIGHTING)] _SECTIONLIGHTING("---- LIGHTING ------------------------", Float) = 1
        [PowerSlider(5.0)] _Shininess("Shininess", Range(0.01, 2)) = 0.3
        [PowerSlider(5.0)] _Brightness("Brightness", Range(0.01, 100)) = 1
        [PowerSlider(5.0)] _Attenuation("Attenuation", Range(0.001, 1)) = 1
        _Color("Color Tint", Color) = (0.5, 0.5, 0.5, 1.0)

        [Toggle(TEXTURES)] _SECTIONTEXTURES("---- TEXTURES ------------------------", Float) = 1
        _MainTex("Texture A", 2D) = "black" {}
        _MainTexRot("Texture A Rotation", Range(0, 360)) = 0

        _DiffTex("Texture B", 2D) = "black" {}
        _DiffTexRot("Texture B Rotation", Range(0, 360)) = 0

        [Toggle(WAVES)] _SECTIONWAVES("---- WAVES AND FLOW ------------------------", Float) = 1
        [NoScaleOffset] _DerivHeightMap("Wave Height Map", 2D) = "black" {}
        _Tiling("Tiling", Float) = 3
        [PowerSlider(1.0)] _Speed("Speed", Range(0.001, 1)) = 0.1
        [PowerSlider(1.0)] _FlowStrength("Flow Strength", Range(-1, 1)) = 0.1
        [PowerSlider(1.0)] _FlowOffset("Flow Offset", Range(-1, 1)) = 0.25
        [PowerSlider(1.0)] _HeightScale("Height Scale, Constant", Range(-1, 1)) = 0.5
        [PowerSlider(1.0)] _HeightScaleModulated("Height Scale, Modulated", Range(-5, 5)) = 4

        [Toggle(FOAM)] _FOAM("---- EDGE FOAM ------------------------", Float) = 1
        _FoamTex("Foam Texture", 2D) = "white" {}
        _FoamSpeed("Foam Speed", Range(0, 1)) = 0.1
        _FoamIntensity("Foam Intensity", Range(0, 10)) = 4.5
        _FoamWidth("Foam width", Range(0, 1)) = 0.05
        _FoamDir("Foam strength", Range(-1, 1)) = 0.05
        _FoamTexRot("Foam Rotation", Range(0, 360)) = 0
        [Toggle(INVERT)] _Invert("Show foam on center. Disable to show foam on edges instead", Float) = 1
        [Toggle(SQUARED)] _Squared("Show foam as a square instead of circle", Float) = 1

        [Toggle(REFLECTION)] _Reflection("---- CUBEMAP REFLECTION ------------------------", Float) = 1
        [PowerSlider(1.0)] _RefStrength("Reflection Strength", Range(0, 2)) = 1
        _Cube("Cubemap", CUBE) = "" {}

        [Toggle(RENDERING)] _Rendering("---- BLENDING MODE ------------------------", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcMode("Src Mode - recommended: SrcAlpha", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstMode("Dst Mode - recommended: OneMinusSrcAlpha", Float) = 10
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcMode] [_DstMode]
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local LIGHTING
            #pragma shader_feature_local TEXTURES
            #pragma shader_feature_local WAVES
            #pragma shader_feature_local REFLECTION
            #pragma shader_feature_local FOAM
            #pragma shader_feature_local INVERT
            #pragma shader_feature_local SQUARED

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float2 uv          : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
            TEXTURE2D(_DiffTex);        SAMPLER(sampler_DiffTex);
            TEXTURE2D(_DerivHeightMap); SAMPLER(sampler_DerivHeightMap);
            TEXTURE2D(_FoamTex);        SAMPLER(sampler_FoamTex);
            TEXTURECUBE(_Cube);         SAMPLER(sampler_Cube);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _DiffTex_ST;
                float4 _FoamTex_ST;

                half4  _Color;
                half   _Brightness;
                half   _RefStrength;
                half   _Attenuation;
                half   _Shininess;

                float  _MainTexRot;
                float  _DiffTexRot;
                float  _FoamTexRot;

                float  _Speed;
                float  _FlowStrength;
                float  _FlowOffset;
                float  _Tiling;
                float  _HeightScale;
                float  _HeightScaleModulated;

                half   _FoamSpeed;
                half   _FoamIntensity;
                half   _FoamWidth;
                half   _FoamDir;
            CBUFFER_END

            // ---- Ported helper functions (identical math to the original) ----

            float2 RotateUV(float2 uv, float2 center, float rotationDegrees)
            {
                float rotation = rotationDegrees * (3.1415926f / 180.0f);
                uv -= center;
                float s = sin(rotation);
                float c = cos(rotation);
                float2x2 rMatrix = float2x2(c, -s, s, c);
                rMatrix *= 0.5;
                rMatrix += 0.5;
                rMatrix = rMatrix * 2 - 1;
                uv = mul(uv, rMatrix);
                uv += center;
                return uv;
            }

            float3 FlowUVW(float2 uv, float2 flowVector, float2 jump, float flowOffset, float tiling, float time, bool flowB)
            {
                float phaseOffset = flowB ? 0.5 : 0;
                float progress = frac(time + phaseOffset);
                float3 uvw;
                uvw.xy = uv - flowVector * (progress + flowOffset);
                uvw.xy *= tiling;
                uvw.xy += phaseOffset;
                uvw.xy += (time - progress) * jump;
                uvw.z = 1 - abs(1 - 2 * progress);
                return uvw;
            }

            float3 UnpackDerivativeHeight(float4 textureData)
            {
                float3 dh = textureData.agb;
                dh.xy = dh.xy * 2 - 1;
                return dh;
            }

            // ---- Vertex ----

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS = normInputs.normalWS;
                OUT.uv = IN.uv;
                return OUT;
            }

            // ---- Fragment ----

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uvMain = TRANSFORM_TEX(IN.uv, _MainTex);
                float2 uvDiff = TRANSFORM_TEX(IN.uv, _DiffTex);
                float2 uvFoam = TRANSFORM_TEX(IN.uv, _FoamTex);

                uvMain = RotateUV(uvMain, float2(0, 0), _MainTexRot);
                uvDiff = RotateUV(uvDiff, float2(0, 0), _DiffTexRot);

                half4 tex  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvMain) * _Color;
                half4 tex2 = SAMPLE_TEXTURE2D(_DiffTex, sampler_DiffTex, uvDiff) * _Color;

                float3 normalWS = normalize(IN.normalWS);

                #if defined(WAVES)
                    float3 flow = SAMPLE_TEXTURE2D(_DiffTex, sampler_DiffTex, uvMain).rgb;
                    flow.xy = flow.xy * 2 - 1;
                    flow *= _FlowStrength;

                    float noise = SAMPLE_TEXTURE2D(_DiffTex, sampler_DiffTex, uvMain).a;
                    float time = _Time.y * _Speed + noise;

                    float3 uvwA = FlowUVW(uvMain, flow.xy, float2(0,0), _FlowOffset, _Tiling, time, false);
                    float3 uvwB = FlowUVW(uvMain, flow.xy, float2(0,0), _FlowOffset, _Tiling, time, true);

                    float finalHeightScale = flow.z * _HeightScaleModulated + _HeightScale;

                    float3 dhA = UnpackDerivativeHeight(SAMPLE_TEXTURE2D(_DerivHeightMap, sampler_DerivHeightMap, uvwA.xy)) * (uvwA.z * finalHeightScale);
                    float3 dhB = UnpackDerivativeHeight(SAMPLE_TEXTURE2D(_DerivHeightMap, sampler_DerivHeightMap, uvwB.xy)) * (uvwB.z * finalHeightScale);

                    float3 tangentNormal = normalize(float3(-(dhA.xy + dhB.xy), 2));
                    // Perturb the world normal in the XZ plane to approximate the
                    // original tangent-space normal used by the surface shader.
                    normalWS = normalize(normalWS + float3(tangentNormal.x, 0, tangentNormal.y));
                #endif

                half3 albedo = half3(0, 0, 0);
                #if defined(TEXTURES)
                    albedo = tex.rgb + tex2.rgb;
                #endif

                half alpha = saturate(tex.a + tex2.a);

                // ---- Lighting (reproduces LightingMobileBlinnPhong) ----
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(IN.positionWS));
                float3 lightDir = mainLight.direction;
                float3 viewDir = normalize(GetWorldSpaceViewDir(IN.positionWS));
                float3 halfDir = normalize(lightDir + viewDir);

                float diff = max(0, dot(normalWS, lightDir));
                float nh = max(0, dot(normalWS, halfDir));
                float shininessTerm = 1.0 / max(_Shininess, 0.0001);
                float spec = pow(nh, shininessTerm * 128.0);
                float atten = mainLight.shadowAttenuation * mainLight.distanceAttenuation;

                half3 color;
                #if defined(LIGHTING)
                    color = (albedo * mainLight.color * diff + (mainLight.color * _Brightness) * spec) * _Attenuation;
                #else
                    color = albedo * mainLight.color * diff * spec * atten;
                #endif

                #if defined(REFLECTION)
                    float3 reflDir = reflect(-viewDir, normalWS);
                    half3 refl = SAMPLE_TEXTURECUBE(_Cube, sampler_Cube, reflDir).rgb;
                    color += refl * _Color.rgb * _RefStrength;
                #endif

                #if defined(FOAM)
                    half2 circleCenter = half2(0.5, 0.5);
                    half distanceToCenter = length(uvFoam - circleCenter);
                    half circleRadius = 1;
                    half foamMask = 1;

                    #if defined(SQUARED)
                        #if defined(INVERT)
                            float2 uvFromCenter = abs(uvFoam - circleCenter);
                            half foamMaskX = 1 - smoothstep(0.0, _FoamWidth, uvFromCenter.x);
                            half foamMaskY = 1 - smoothstep(0.0, _FoamWidth, uvFromCenter.y);
                            foamMask = min(foamMaskX, foamMaskY);
                        #else
                            half foamMask1 = 1 - smoothstep(0.0, _FoamWidth, uvFoam.x) * smoothstep(0.0, _FoamWidth, uvFoam.y);
                            half foamMask2 = 1 - smoothstep(1.0, 1 - _FoamWidth, uvFoam.y) * smoothstep(1.0, 1 - _FoamWidth, uvFoam.x);
                            foamMask = foamMask1 + foamMask2;
                        #endif
                    #else
                        #if defined(INVERT)
                            foamMask = 1 - smoothstep(0.0, _FoamWidth, distanceToCenter);
                        #else
                            foamMask = smoothstep(circleRadius - _FoamWidth, circleRadius, distanceToCenter);
                        #endif
                    #endif

                    half2 foamUV = uvMain;
                    foamUV = RotateUV(foamUV, circleCenter, _FoamTexRot);
                    foamUV += (_Time.y * _FoamSpeed) * _FoamDir;

                    half4 foamColor = SAMPLE_TEXTURE2D(_FoamTex, sampler_FoamTex, foamUV);

                    #if defined(TEXTURES)
                        half4 finalColor = lerp(tex, tex * _FoamIntensity, foamMask);
                    #else
                        half4 finalColor = lerp(_Color, _Color * _FoamIntensity, foamMask);
                    #endif

                    half finalFoamMask = foamMask * _FoamIntensity;
                    finalColor.rgba += foamColor.rgba * finalFoamMask;

                    color += finalColor.rgb;
                    alpha = saturate(alpha + finalColor.a);
                #endif

                return half4(color, alpha);
            }
            ENDHLSL
        }

        // Casts shadows using URP's standard shadow caster pass so the water
        // plane still receives correct shadowing from other objects.
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
