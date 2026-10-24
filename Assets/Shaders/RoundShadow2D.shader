Shader "Custom/2DRoundShadowStencil"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Opacity ("Opacity", Range(0, 1)) = 0.5
        _Pixels ("Pixels", Float) = 16.0
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Stencil
        {
            Ref 1
            Comp NotEqual
            Pass Replace
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos    : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _Opacity;
                float _Pixels;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);

                output.positionHCS = TransformWorldToHClip(worldPos);
                output.worldPos = worldPos;
                return output;
            }

            float CalculateEllipse(float2 uv, float width, float height)
            {
                float2 d = abs(uv * 2.0 - 1.0);
                float l = length(d / float2(width, height));
                return saturate((1.0 - l) / max(fwidth(l), 0.0001));
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 worldPos = input.worldPos;

                if (_Pixels > 0.0)
                {
                    worldPos = floor(worldPos * _Pixels) / _Pixels;
                }

                float3 objPos = TransformWorldToObject(worldPos);

                float2 shadowUV = objPos.xy + float2(0.5, 0.5);

                float ellipseAlpha = CalculateEllipse(shadowUV, 0.95, 0.9);

                clip(ellipseAlpha - 0.01f);

                return half4(0.0, 0.0, 0.0, ellipseAlpha * _Opacity);
            }
            ENDHLSL
        }
    }
}