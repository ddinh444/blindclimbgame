Shader "Custom/NewUnlitUniversalRenderPipelineShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _OutlineThickness("Outline Thickness", Range(0,0.1)) = 0.03
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct AudioSourceData
            {
                float3 position;
                float radius;
                float  ttl;
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

            float EaseOutQuad(float x) {
                return 1.0 - (1.0 - x) * (1.0 - x);
            }

            float ComputeEcholocation(float3 worldPos)
            {
                float accum = 0.0;

                [loop]
                for (int i = 0; i < _AudioSourceCount; i++)
                {
                    AudioSourceData src = _AudioSources[i];

                    float d = distance(worldPos, src.position);
                    float delta = abs(d - src.radius);

                    float ring = 1.0 - smoothstep(0.0, _RingWidth, delta);

                    float ageFade = EaseOutQuad(src.ttl);

                    accum += ring * ageFade;
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
            Cull front
            
            HLSLPROGRAM
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END
            float _OutlineThickness;

            struct appdata{
                float4 vertex : POSITION;  
                float4 normal : NORMAL;
            };


            struct v2f{
                float4 position : SV_POSITION;
                float3 posWS : TEXCOORD0;
            };

            v2f vert(appdata IN){
                v2f o;
                float4 normal = normalize(IN.normal);
                float4 offset = normal * _OutlineThickness;
                float4 position = IN.vertex + offset;

                o.position = TransformObjectToHClip(position.xyz);
                o.posWS =  mul(unity_ObjectToWorld, position).xyz;

                return o;
            }

            half4 frag (v2f IN) : SV_Target{
                return _BaseColor;
            }

            ENDHLSL
        }
    }
}
