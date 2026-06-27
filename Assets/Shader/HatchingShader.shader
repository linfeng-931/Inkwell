Shader "Custom/Hatching_RealLit_Transparent_SceneColor"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _BaseMap("Base Map", 2D) = "white" {}
        _Hatch0("Hatch (Light)", 2D) = "white" {}
        _Hatch1("Hatch (Dark)", 2D) = "white" {}
        _HatchScale("Hatch Scale", Float) = 10.0
        _HatchAngle("Hatch Angle (旋轉角度)", Range(0, 360)) = 45.0
        
        _HatchColor0("Hatch Light Color", Color) = (0.2, 0.3, 0.5, 1)
        _HatchColor1("Hatch Dark Color", Color) = (0.1, 0.1, 0.3, 1)    

        [Header(Softness Settings)]
        _Hatch0_Start("Hatch0 出現點 (亮部)", Range(0, 1)) = 0.2
        _Hatch0_End("Hatch0 消失點", Range(0, 1)) = 0.6
        _Hatch1_Start("Hatch1 出現點 (暗部)", Range(0, 1)) = 0.0
        _Hatch1_End("Hatch1 消失點", Range(0, 1)) = 0.3

        [Header(Light Sensitivity)]
        _LightSensitivity("Spot Light 靈敏度", Range(0.5, 5000.0)) = 100.0

        [Header(Underlying Material Settings)]
        _BgShadowInfluence("下層陰影/材質對排線密度的影響", Range(0, 1)) = 0.7
    }

    SubShader
    {
        Tags { 
            "RenderType" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend DstColor Zero
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS _ADDITIONAL_LIGHTS_VERTEX
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            TEXTURE2D(_BaseMap);   SAMPLER(sampler_BaseMap);
            TEXTURE2D(_Hatch0);    SAMPLER(sampler_Hatch0);
            TEXTURE2D(_Hatch1);    SAMPLER(sampler_Hatch1);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _HatchScale;
                half _HatchAngle;
                half _Hatch0_Start;
                half _Hatch0_End;
                half _Hatch1_Start;
                half _Hatch1_End;
                half4 _HatchColor0;
                half4 _HatchColor1;
                half _LightSensitivity;
                half _BgShadowInfluence;
            CBUFFER_END

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL; 
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            float2 RotateUV(float2 uv, float angleDegrees) {
                float angleRadians = angleDegrees * 3.14159265 / 180.0;
                float s = sin(angleRadians);
                float c = cos(angleRadians);
                return float2(uv.x * c - uv.y * s, uv.x * s + uv.y * c);
            }

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target {
                float2 screenUV = GetNormalizedScreenSpaceUV(IN.positionHCS);

                // 抓取並計算下層材質的明暗度
                half3 bgColor = SampleSceneColor(screenUV);
                half bgLuminance = saturate(dot(bgColor, half3(0.2126, 0.7152, 0.0722)));

                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalizedScreenSpaceUV = screenUV;

                float3 normalWS = normalize(IN.normalWS);
                
                // --- 燈光強度計算 ---
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                
                half mainNDotL = saturate(dot(normalWS, mainLight.direction));
                half mainLightLuminance = dot(mainLight.color, half3(0.2126, 0.7152, 0.0722));
                half mainLightIntensity = mainNDotL * mainLight.distanceAttenuation * mainLight.shadowAttenuation * mainLightLuminance;

                half additionalLightIntensity = 0;
                #if defined(_ADDITIONAL_LIGHTS) || defined(_FORWARD_PLUS) || defined(_CLUSTER_LIGHT_LOOP)
                    uint pixelLightCount = GetAdditionalLightsCount();
                    LIGHT_LOOP_BEGIN(pixelLightCount)
                        Light light = GetAdditionalLight(lightIndex, IN.positionWS, shadowCoord); 
                        half addNDotL = saturate(dot(normalWS, light.direction));
                        half addLightLuminance = dot(light.color, half3(0.2126, 0.7152, 0.0722));
                        additionalLightIntensity += addNDotL * light.distanceAttenuation * light.shadowAttenuation * addLightLuminance * _LightSensitivity;
                    LIGHT_LOOP_END
                #endif

                half totalLight = saturate(mainLightIntensity + additionalLightIntensity);

                // 融入下層明暗度與陰影
                totalLight = saturate(totalLight * lerp(1.0, bgLuminance, _BgShadowInfluence));

                // --- 三平面排線 UV 旋轉計算 ---
                float3 blending = abs(normalWS);
                blending /= (blending.x + blending.y + blending.z);

                float2 uvX = RotateUV(IN.positionWS.zy * _HatchScale, _HatchAngle);
                float2 uvY = RotateUV(IN.positionWS.xz * _HatchScale, _HatchAngle);
                float2 uvZ = RotateUV(IN.positionWS.xy * _HatchScale, _HatchAngle);

                // 採樣與混合排線顏色
                half3 h0x = SAMPLE_TEXTURE2D(_Hatch0, sampler_Hatch0, uvX).rgb;
                half3 h0y = SAMPLE_TEXTURE2D(_Hatch0, sampler_Hatch0, uvY).rgb;
                half3 h0z = SAMPLE_TEXTURE2D(_Hatch0, sampler_Hatch0, uvZ).rgb;
                half3 h0_mask = h0x * blending.x + h0y * blending.y + h0z * blending.z;
                half3 h0_final = lerp(_HatchColor0.rgb, half3(1, 1, 1), h0_mask.r);

                half3 h1x = SAMPLE_TEXTURE2D(_Hatch1, sampler_Hatch1, uvX).rgb;
                half3 h1y = SAMPLE_TEXTURE2D(_Hatch1, sampler_Hatch1, uvY).rgb;
                half3 h1z = SAMPLE_TEXTURE2D(_Hatch1, sampler_Hatch1, uvZ).rgb;
                half3 h1_mask = h1x * blending.x + h1y * blending.y + h1z * blending.z;
                half3 h1_final = lerp(_HatchColor1.rgb, half3(1, 1, 1), h1_mask.r);

                half3 combinedHatch = half3(1, 1, 1);
                combinedHatch *= lerp(h0_final, half3(1, 1, 1), smoothstep(_Hatch0_Start, _Hatch0_End, totalLight));
                combinedHatch *= lerp(h1_final, half3(1, 1, 1), smoothstep(_Hatch1_Start, _Hatch1_End, totalLight));

                half4 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                half3 finalRGB = baseCol.rgb * combinedHatch;
                
                return half4(finalRGB, 1.0);
            }
            ENDHLSL
        }
    }
}