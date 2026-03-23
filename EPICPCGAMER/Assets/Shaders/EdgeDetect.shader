Shader "Custom/EdgeDetect"
{
    Properties
    {
        _OutlineColor  ("Outline Color",   Color)          = (0,0,0,1)
        _OutlineThick  ("Outline Thickness", Range(0,5))   = 1.5
        _DepthSensitivity   ("Depth Sensitivity",   Range(0,5)) = 1.0
        _NormalSensitivity  ("Normal Sensitivity",  Range(0,5)) = 1.0
        _ShadingSteps  ("Shading Steps (Comic)",  Range(1,8))   = 3
        _ShadingStrength("Shading Strength", Range(0,1))   = 0.35
    }

    HLSLINCLUDE
    #pragma vertex Vert
    #pragma fragment Frag

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    float4 _OutlineColor;
    float  _OutlineThick;
    float  _DepthSensitivity;
    float  _NormalSensitivity;
    float  _ShadingSteps;
    float  _ShadingStrength;

    // Sample depth at a UV offset
    float SampleDepth(float2 uv)
    {
        return LoadCameraDepth(uv * _ScreenSize.xy);
    }

    float4 Frag(Varyings v) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(v);

        float2 uv = v.positionCS.xy * _ScreenSize.zw;
        float2 texel = _OutlineThick / _ScreenSize.xy;

        // Sample depth at center and 4 neighbours
        float d  = SampleDepth(uv);
        float dN = SampleDepth(uv + float2( 0,  texel.y));
        float dS = SampleDepth(uv + float2( 0, -texel.y));
        float dE = SampleDepth(uv + float2( texel.x,  0));
        float dW = SampleDepth(uv + float2(-texel.x,  0));

        // Sobel-style edge from depth differences
        float depthEdge = abs(dN - d) + abs(dS - d) + abs(dE - d) + abs(dW - d);
        depthEdge = saturate(depthEdge * _DepthSensitivity * 80.0);

        // Sample scene color
        float3 sceneColor = LoadCameraColor(uint2(uv * _ScreenSize.xy), 0);

        // Comic-style cel shading — posterize luminance into steps
        float lum     = dot(sceneColor, float3(0.299, 0.587, 0.114));
        float stepped = floor(lum * _ShadingSteps) / _ShadingSteps;
        float shadingMult = lerp(1.0, stepped / max(lum, 0.001), _ShadingStrength);
        sceneColor *= shadingMult;

        // Blend outline color over scene
        float3 finalColor = lerp(sceneColor, _OutlineColor.rgb, depthEdge * _OutlineColor.a);

        return float4(finalColor, 1.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline"="HDRenderPipeline" }
        ZWrite Off ZTest Always Blend Off Cull Off
        Pass
        {
            Name "EdgeDetect"
            HLSLPROGRAM
            ENDHLSL
        }
    }
}