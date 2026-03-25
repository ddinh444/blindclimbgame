Shader "Custom/EcholocationShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)

        _RingWidth ("Ring Width", Float) = 0.25
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define MAX_AUDIO_SOURCES 10

            struct AudioSourceData
            {
                float3 position;
                float radius;
                float strength;
            };

            StructuredBuffer<AudioSourceData> _AudioSources;
            int _AudioSourceCount;

            float _RingWidth;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS  = mul(unity_ObjectToWorld, IN.positionOS).xyz;
                return OUT;
            }

            float ComputeEcholocation(float3 worldPos)
            {
                float accum = 0.0;

                [unroll]
                for (int i = 0; i < MAX_AUDIO_SOURCES; i++)
                {
                    AudioSourceData src = _AudioSources[i];

                    //if distance from 
                    float d = distance(worldPos, src.position);
                    float delta = abs(d - src.radius);

                    float ring = 1.0 - smoothstep(0.0, _RingWidth, delta);

                    accum += ring * src.strength;
                }

                return saturate(accum);
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 outColor = half4(0,0,0,1);

                float echo = ComputeEcholocation(IN.positionWS);

                outColor.rgb += echo * _BaseColor.rgb;

                return outColor;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormals" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                half3 normal : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normal : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normal = TransformObjectToWorldNormal(IN.normal);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return half4(IN.normal,1);
            }

            ENDHLSL
        }
    }
}
