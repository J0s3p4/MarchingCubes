
// To use, on the Material, Under Shader, go to Custom -> Terrain

Shader "Custom/URP_Terrain" {
    Properties{
        _MainTex("Ground Texture", 2D) = "white" {}
        _WallTex("Wall Texture", 2D) = "white" {}
        _TexScale("Texture Scale", Float) = 1
    }
    SubShader{
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_WallTex);
            SAMPLER(sampler_WallTex);
            float _TexScale;

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS).xyz;
                OUT.worldNormal = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target {
                float3 scaledWorldPos = IN.worldPos / _TexScale;
                float3 pWeight = abs(IN.worldNormal);
                pWeight /= (pWeight.x + pWeight.y + pWeight.z);

                float3 xP = SAMPLE_TEXTURE2D(_WallTex, sampler_WallTex, scaledWorldPos.yz).rgb * pWeight.x;
                float3 yP = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, scaledWorldPos.xz).rgb * pWeight.y;
                float3 zP = SAMPLE_TEXTURE2D(_WallTex, sampler_WallTex, scaledWorldPos.xy).rgb * pWeight.z;

                float3 albedo = xP + yP + zP;
                return half4(albedo, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Forward"
}