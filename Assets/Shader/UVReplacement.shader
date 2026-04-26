Shader "Hidden/UVReplacement"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" } // 確保只渲染不透明物體

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target {
                // 將 UV 座標存入 R 和 G 通道，B 設為 0，Alpha 設為 1
                // 這樣後製 Shader 就能透過 Alpha 通道判斷哪裡有物體
                return half4(IN.uv.x, IN.uv.y, 0, 1);
            }
            ENDHLSL
        }
    }
}