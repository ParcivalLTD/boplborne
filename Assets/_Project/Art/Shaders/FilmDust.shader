// Grace Run film dust: pixel-sized specks and thin vertical scratches on a base-resolution grid
// (_Grid, e.g. 960x540), a new pattern per _Seed. Drawn on a full-screen UI RawImage by FilmDust,
// which steps _Seed at the dust fps. Fully procedural; no texture.
Shader "GraceRun/Film Dust"
{
    Properties
    {
        [HideInInspector] _MainTex("Texture", 2D) = "white" {}
        _Grid("Pixel Grid", Vector) = (960, 540, 0, 0)
        _Seed("Seed", Float) = 0
        _Opacity("Opacity", Range(0, 1)) = 0.35
        _Density("Speck Density", Range(0, 0.01)) = 0.0004
        _LightShare("Light Speck Share", Range(0, 1)) = 0.35
        _ScratchChance("Scratch Chance", Range(0, 1)) = 0.25
        _DarkColor("Dark Speck", Color) = (0.07, 0.06, 0.09, 1)
        _LightColor("Light Speck", Color) = (0.95, 0.91, 0.84, 1)
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" }
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Grid;
                float _Seed;
                half _Opacity;
                half _Density;
                half _LightShare;
                half _ScratchChance;
                half4 _DarkColor;
                half4 _LightColor;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            uint DustHash(uint x)
            {
                x ^= x >> 16; x *= 0x7feb352dU;
                x ^= x >> 15; x *= 0x846ca68bU;
                x ^= x >> 16;
                return x;
            }

            float DustRand(uint a, uint b, uint c) { return DustHash(a ^ DustHash(b ^ DustHash(c))) / 4294967295.0; }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                uint2 cell = (uint2)floor(saturate(input.uv) * (_Grid.xy - 1));
                uint seed = (uint)_Seed;

                // Specks: single pixels, plus rarer 2x2 clumps on the half grid.
                float fine = DustRand(cell.x, cell.y, seed);
                float coarse = DustRand(cell.x >> 1, cell.y >> 1, seed + 7919u);
                bool speck = fine < _Density || coarse < _Density * 0.25;
                float tone = DustRand(cell.x >> 1, cell.y >> 1, seed + 104729u);

                // Up to two scratches: a one-pixel column with a vertical run, light like scratched emulsion.
                bool scratch = false;
                [unroll] for (uint i = 0u; i < 2u; i++)
                {
                    bool on = DustRand(seed, i, 31u) < _ScratchChance;
                    uint column = (uint)(DustRand(seed, i, 37u) * _Grid.x);
                    float centre = DustRand(seed, i, 41u);
                    float extent = 0.2 + 0.4 * DustRand(seed, i, 43u);
                    scratch = scratch || (on && cell.x == column && abs(input.uv.y - centre) <= extent);
                }

                if (scratch) return half4(_LightColor.rgb, _Opacity * 0.6);
                if (!speck) return half4(0, 0, 0, 0);
                half3 color = tone < _LightShare ? _LightColor.rgb : _DarkColor.rgb;
                return half4(color, _Opacity);
            }
            ENDHLSL
        }
    }
}
