Shader "Custom/Hatching_RealLit"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _BaseMap("Base Map", 2D) = "white" {}
        _Hatch0("Hatch (Light)", 2D) = "white" {}
        _Hatch1("Hatch (Dark)", 2D) = "white" {}
        _HatchScale("Hatch Scale", Float) = 10.0
        _ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.5
        _HatchColor0("Hatch Light Color", Color) = (0.2, 0.3, 0.5, 1)
        _HatchColor1("Hatch Dark Color", Color) = (0.1, 0.1, 0.3, 1)    

        [Header(Softness Settings)]
        _Hatch0_Start("Hatch0 出現點 (亮部)", Range(0, 1)) = 0.2
        _Hatch0_End("Hatch0 消失點", Range(0, 1)) = 0.6
        _Hatch1_Start("Hatch1 出現點 (暗部)", Range(0, 1)) = 0.0
        _Hatch1_End("Hatch1 消失點", Range(0, 1)) = 0.3
    }

    SubShader
    {
        Tags { 
            "RenderType" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Transparent+1"
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

            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);   SAMPLER(sampler_BaseMap);
            TEXTURE2D(_Hatch0);    SAMPLER(sampler_Hatch0);
            TEXTURE2D(_Hatch1);    SAMPLER(sampler_Hatch1);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _HatchScale;
                half _Hatch0_Start;
                half _Hatch0_End;
                half _Hatch1_Start;
                half _Hatch1_End;
                half4 _HatchColor0;
                half4 _HatchColor1;
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

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target {
                float3 normalWS = normalize(IN.normalWS);
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                
                //light
                Light mainLight = GetMainLight(shadowCoord);
                half d = dot(normalWS, mainLight.direction) * 0.5 + 0.5;
                half mainLightIntensity = d * mainLight.distanceAttenuation * mainLight.shadowAttenuation;

                half additionalLightIntensity = 0;
                #ifdef _ADDITIONAL_LIGHTS
                    uint pixelLightCount = GetAdditionalLightsCount();
                    for (uint i = 0; i < pixelLightCount; ++i) {
                        Light light = GetAdditionalLight(i, IN.positionWS, shadowCoord);
                        half addD = dot(normalWS, light.direction) * 0.5 + 0.5;
                        additionalLightIntensity += addD * light.distanceAttenuation * light.shadowAttenuation;
                    }
                #endif

                half totalLight = mainLightIntensity + additionalLightIntensity;

                //三平面排線 UV 計算
                float3 blending = abs(normalWS);
                blending /= (blending.x + blending.y + blending.z);

                float2 uvX = IN.positionWS.zy * _HatchScale;
                float2 uvY = IN.positionWS.xz * _HatchScale;
                float2 uvZ = IN.positionWS.xy * _HatchScale;

                //套用顏色
                half3 h0x = SAMPLE_TEXTURE2D(_Hatch0, sampler_Hatch0, uvX).rgb;
                half3 h0y = SAMPLE_TEXTURE2D(_Hatch0, sampler_Hatch0, uvY).rgb;
                half3 h0z = SAMPLE_TEXTURE2D(_Hatch0, sampler_Hatch0, uvZ).rgb;
                half3 h0_mask = h0x * blending.x + h0y * blending.y + h0z * blending.z;
                half3 h0_final = lerp(_HatchColor0.rgb, half3(1, 1, 1), h0_mask.r);

                half3 h1x = SAMPLE_TEXTURE2D(_Hatch1, sampler_Hatch1, uvX).rgb;
                half3 h1y = SAMPLE_TEXTURE2D(_Hatch1, sampler_Hatch1, uvY).rgb;
                half3 h1z = SAMPLE_TEXTURE2D(_Hatch1, sampler_Hatch1, uvZ).rgb;
                half3 h1_mask = h1x * blending.x + h1y * blending.y + h1z * blending.z;

                //output
                half3 h1_final = lerp(_HatchColor1.rgb, half3(1, 1, 1), h1_mask.r);

                half3 combinedHatch = half3(1, 1, 1);
                combinedHatch *= lerp(h0_final, 1.0, smoothstep(_Hatch0_Start, _Hatch0_End, totalLight));
                combinedHatch *= lerp(h1_final, 1.0, smoothstep(_Hatch1_Start, _Hatch1_End, totalLight));

                half4 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                
                half3 finalRGB = baseCol.rgb * combinedHatch * (totalLight + 0.5);
                
                return half4(finalRGB, 1.0);
            }
            ENDHLSL
        }
    }
}