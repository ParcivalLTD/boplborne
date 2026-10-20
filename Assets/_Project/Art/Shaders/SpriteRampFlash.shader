// Grace Run sprite shader (URP 2D, lit like Sprite-Lit-Default).
// Ramp swap: texels that match P1's blue ramp (_RampFrom0-2) become the player's ramp (_RampTo0-2).
// Hit flash: _FlashAmount 0-1 fades the tinted sprite to _FlashColor, keeping its alpha.
// _RampTo* and _FlashAmount are set per renderer through a MaterialPropertyBlock
// (PlayerVisuals, SpriteFlash). Bosses, dummy and summons use the flash-only material.
Shader "GraceRun/Sprite Ramp Flash"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}

        [Toggle(_RAMP_SWAP)] _RampSwap("Ramp Swap", Float) = 0
        _RampFrom0("Ramp From Dark", Color) = (0.1686, 0.3569, 0.7529, 1)
        _RampFrom1("Ramp From Mid", Color) = (0.2980, 0.5529, 1, 1)
        _RampFrom2("Ramp From Light", Color) = (0.7490, 0.8392, 1, 1)
        _RampTo0("Ramp To Dark", Color) = (0.1686, 0.3569, 0.7529, 1)
        _RampTo1("Ramp To Mid", Color) = (0.2980, 0.5529, 1, 1)
        _RampTo2("Ramp To Light", Color) = (0.7490, 0.8392, 1, 1)
        _RampTolerance("Ramp Match Tolerance", Range(0, 0.1)) = 0.01

        _FlashColor("Flash Color", Color) = (1, 1, 1, 1)
        _FlashAmount("Flash Amount", Range(0, 1)) = 0

        [MaterialToggle] _ZWrite("ZWrite", Float) = 0

        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    HLSLINCLUDE
    // Shared by every pass so the SRP batcher sees one layout.
    #define GRACE_SPRITE_PROPERTIES \
        CBUFFER_START(UnityPerMaterial) \
            half4 _Color; \
            half4 _RampFrom0; half4 _RampFrom1; half4 _RampFrom2; \
            half4 _RampTo0; half4 _RampTo1; half4 _RampTo2; \
            half _RampTolerance; \
            half4 _FlashColor; \
            half _FlashAmount; \
        CBUFFER_END

    #define GRACE_RAMP_SWAP(texel) \
        texel = (distance(texel, _RampFrom0.rgb) < _RampTolerance) ? _RampTo0.rgb : texel; \
        texel = (distance(texel, _RampFrom1.rgb) < _RampTolerance) ? _RampTo1.rgb : texel; \
        texel = (distance(texel, _RampFrom2.rgb) < _RampTolerance) ? _RampTo2.rgb : texel;
    ENDHLSL

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex LitVertex
            #pragma fragment LitFragment

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"

            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile _ SKINNED_SPRITE
            #pragma shader_feature_local _RAMP_SWAP

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color        : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_LIT_OUTPUTS
                half4 color        : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Lit2DCommon.hlsl"

            GRACE_SPRITE_PROPERTIES

            Varyings LitVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonLitVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }

            half4 LitFragment(Varyings input) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                #if defined(_RAMP_SWAP)
                GRACE_RAMP_SWAP(tex.rgb)
                #endif
                half4 main = input.color * tex;
                main.rgb = lerp(main.rgb, _FlashColor.rgb, _FlashAmount);

                const half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv);
                const half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));

                SurfaceData2D surfaceData;
                InputData2D inputData;
                InitializeSurfaceData(main.rgb, main.a, mask, normalTS, surfaceData);
                InitializeInputData(input.uv, input.lightingUV, inputData);

                #if defined(DEBUG_DISPLAY)
                SETUP_DEBUG_TEXTURE_DATA_2D_NO_TS(inputData, input.positionWS, input.positionCS, _MainTex);
                surfaceData.normalWS = input.normalWS;
                #endif

                return CombinedShapeLightShared(surfaceData, inputData);
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "NormalsRendering"}

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex NormalsRenderingVertex
            #pragma fragment NormalsRenderingFragment

            #pragma multi_compile_instancing
            #pragma multi_compile _ SKINNED_SPRITE

            struct Attributes
            {
                COMMON_2D_NORMALS_INPUTS
                float4 color        : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_NORMALS_OUTPUTS
                half4   color           : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Normals2DCommon.hlsl"

            GRACE_SPRITE_PROPERTIES

            Varyings NormalsRenderingVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonNormalsVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }

            half4 NormalsRenderingFragment(Varyings input) : SV_Target
            {
                SetUpSpriteInstanceProperties();
                return CommonNormalsFragment(input, input.color);
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue"="Transparent" "RenderType"="Transparent"}

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"

            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE
            #pragma shader_feature_local _RAMP_SWAP

            GRACE_SPRITE_PROPERTIES

            Varyings UnlitVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonUnlitVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }

            half4 UnlitFragment(Varyings input) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                #if defined(_RAMP_SWAP)
                GRACE_RAMP_SWAP(tex.rgb)
                #endif
                half4 main = input.color * tex;
                main.rgb = lerp(main.rgb, _FlashColor.rgb, _FlashAmount);
                return main;
            }
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/2D/Sprite-Lit-Default"
}
