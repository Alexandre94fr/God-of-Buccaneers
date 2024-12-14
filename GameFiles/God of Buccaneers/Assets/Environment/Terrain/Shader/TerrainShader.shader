
Shader "ProgGen/TerrainShaderURP"
{
    Properties
    {
        _Color ("Color Tint", Color) = (1,1,1,1)

        _LowTexture ("Sand Texture (RGB)", 2D) = "white" {}
        _MidTexture ("Dirt Texture (RGB)", 2D) = "white" {}
        _HighTexture ("Rock Texture (RGB)", 2D) = "white" {}

        _LowThreshold ("Low Threshold", Range(0, 50)) = 0.33
        _HighThreshold ("High Threshold", Range(0, 50)) = 0.66

        _Blend ("Blend Factor", Range(0, 1)) = 0.05
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" "RenderType" = "Opaque" }
        LOD 200

        Pass
        {
            Name "MainPass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            //#pragma vertex vert
            //#pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Structures d'entrée et de sortie
            struct VertexInput
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                float4 vertexColor : COLOR; // Hauteur passée via couleur des sommets
            };

            struct FragmentInput
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float height : TEXCOORD1; // Hauteur calculée en vertex
            };

            // Déclarations des propriétés
            /*
            TEXTURE2D(_LowTexture)
            TEXTURE2D(_MidTexture)
            TEXTURE2D(_HighTexture)

            SAMPLER(sampler_LowTexture)
            SAMPLER(sampler_MidTexture)
            SAMPLER(sampler_HighTexture)

            CBUFFER_START(UnityPerMaterial)

            float4 _Color;

            float _LowThreshold;
            float _HighThreshold;

            float _Blend;

            float4 _LowTexture_ST;
            float4 _MidTexture_ST;
            float4 _HighTexture_ST;

            CBUFFER_END
            
            // Vertex Shader
            FragmentInput vert(VertexInput vertexInput)
            {
                FragmentInput fragmentInput;

                fragmentInput.position = mul(UNITY_MATRIX_MVP, vertexInput.position); // Transformation standard
                fragmentInput.uv = TRANSFORM_TEX(vertexInput.uv, _LowTexture);

                fragmentInput.height = vertexInput.vertexColor.r; // Canal rouge utilisé pour la hauteur

                return fragmentInput;
            }

            // Fragment Shader
            float4 frag(FragmentInput fragmentInput) : SV_Target
            {
                // Retrieve the height from the vertex color
                float height = fragmentInput.height;
            
                // Calculate blending factors
                float lowTexture = 1.0 - smoothstep(_LowThreshold, _LowThreshold + _Blend, height); // Sand layer
                float highTexture = smoothstep(_HighThreshold, _HighThreshold + _Blend, height);    // Rock layer
            
                // Mid-layer blending: combines lower and upper thresholds
                float blendMidLow = smoothstep(_LowThreshold, _LowThreshold + _Blend, height);
                float blendMidHigh = 1.0 - smoothstep(_HighThreshold, _HighThreshold + _Blend, height);
                
                float middleTexture = blendMidLow * blendMidHigh; // Dirt layer
            
                // Sample textures and apply the color tint
                half4 lowColor     = SAMPLE_TEXTURE2D(_LowTexture, sampler_LowTexture, fragmentInput.uv) * _Color;
                half4 middleColor  = SAMPLE_TEXTURE2D(_MidTexture, sampler_MidTexture, fragmentInput.uv) * _Color;
                half4 highColor    = SAMPLE_TEXTURE2D(_HighTexture, sampler_HighTexture, fragmentInput.uv) * _Color;
            
                // Combine textures based on blending factors
                return lowColor * lowTexture + middleColor * middleTexture + highColor * highTexture;
            }
            */
            ENDHLSL
        }
    }
}
