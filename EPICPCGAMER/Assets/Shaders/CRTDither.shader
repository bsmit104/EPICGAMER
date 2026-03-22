Shader "Custom/CRTDither_HDRP"
{
    Properties
    {
        _ScanlineScroll  ("Scanline Scroll",  Range(0,1))   = 0
        _DitherStrength  ("Dither Strength",  Range(0,1))   = 0.45
        _PhosphorGreen   ("Phosphor Green",   Range(0,0.3)) = 0.08
        _Brightness      ("Brightness",       Range(0.5,1.5)) = 1.0
        _VignettePower   ("Vignette Power",   Range(0,4))   = 1.8
        _CurvatureX      ("Curvature X",      Range(0,0.15)) = 0.04
        _CurvatureY      ("Curvature Y",      Range(0,0.15)) = 0.04
    }

    HLSLINCLUDE
    #pragma vertex Vert
    #pragma fragment Frag

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    float _ScanlineScroll;
    float _DitherStrength;
    float _PhosphorGreen;
    float _Brightness;
    float _VignettePower;
    float _CurvatureX;
    float _CurvatureY;

    float Bayer4x4(float2 p)
    {
        int2 ip = int2(fmod(abs(p), 4.0));
        const float b[16] = {
             0,  8,  2, 10,
            12,  4, 14,  6,
             3, 11,  1,  9,
            15,  7, 13,  5
        };
        return b[ip.y * 4 + ip.x] / 16.0;
    }

    float2 CRTCurve(float2 uv)
    {
        uv = uv * 2.0 - 1.0;
        float2 off = abs(uv.yx) * float2(_CurvatureX, _CurvatureY);
        uv = uv + uv * off * off;
        return uv * 0.5 + 0.5;
    }

    float4 Frag(Varyings v) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(v);

        float2 uv  = v.positionCS.xy * _ScreenSize.zw;
        float2 cuv = CRTCurve(uv);

        float edge = (cuv.x > 0.0 && cuv.x < 1.0 &&
                      cuv.y > 0.0 && cuv.y < 1.0) ? 1.0 : 0.0;

        float2 samplePos = cuv * _ScreenSize.xy;
        float4 col = float4(LoadCameraColor(uint2(samplePos), 0), 1.0);

        float bayer = Bayer4x4(samplePos);
        float lum   = dot(col.rgb, float3(0.299, 0.587, 0.114));
        float dith  = floor(lum * 4.0 + bayer) / 4.0;
        col.rgb     = lerp(col.rgb, col.rgb * (dith / max(lum, 0.001)), _DitherStrength);

        col.g += _PhosphorGreen * col.g;

        float sl = frac(cuv.y * 240.0 + _ScanlineScroll);
        sl       = smoothstep(0.0, 0.3, sl) * smoothstep(1.0, 0.7, sl);
        col.rgb *= lerp(0.75, 1.0, sl);

        float2 vigUV = uv * 2.0 - 1.0;
        float  vig   = 1.0 - dot(vigUV, vigUV) * _VignettePower * 0.25;
        col.rgb     *= saturate(vig);

        col.rgb *= _Brightness * edge;
        return col;
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline"="HDRenderPipeline" }
        ZWrite Off ZTest Always Blend Off Cull Off
        Pass
        {
            Name "CRTPass"
            HLSLPROGRAM
            ENDHLSL
        }
    }
}