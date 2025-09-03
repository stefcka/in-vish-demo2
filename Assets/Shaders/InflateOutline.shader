Shader "Hidden/InflateOutline"
{
    Properties
    {
        _Width ("Width", Float) = 0.05
        _Color ("Color", Color) = (1,1,0,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="HDRenderPipeline" "Queue"="Overlay+1" }
        Pass
        {
            Name "Outline"
            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Width;
            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                worldPos += worldNormal * _Width;
                o.pos = UnityWorldToClipPos(float4(worldPos, 1));
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }
    }
}

/*
Shader "Custom/SilhouetteOutline"
{
    Properties
    {
        _Thickness ("Outline Thickness", Float) = 1
        _EdgeColor ("Edge Color", Color) = (1,1,0,1)
        _DepthThreshold ("Depth Threshold", Float) = 0.01
        _NormalThreshold ("Normal Threshold", Float) = 0.4
    }
    HLSLINCLUDE

    #pragma vertex Vert
    #pragma fragment Frag
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
    #pragma target 4.5

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    float _Thickness;
    float4 _EdgeColor;
    float _DepthThreshold;
    float _NormalThreshold;

    float CompareDepth(float2 uv, float depthCenter)
    {
        float depthSample = SampleCameraDepth(uv);
        float d = abs(depthSample - depthCenter);
        return step(_DepthThreshold, d);
    }

    float CompareNormal(float2 uv, float3 normalCenter)
    {
        float3 normalSample = SampleCameraNormal(uv);
        float dotVal = dot(normalSample, normalCenter);
        return step(1.0 - _NormalThreshold, 1.0 - dotVal);
    }

    float4 Frag(Varyings varyings) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);

        float2 uv = varyings.positionCS.xy / _ScreenSize.xy;
        float depthCenter = SampleCameraDepth(uv);
        float3 normalCenter = SampleCameraNormal(uv);

        float outline = 0;
        float2 pixelSize = _Thickness / _ScreenSize.xy;

        outline += CompareDepth(uv + float2(-pixelSize.x, 0), depthCenter);
        outline += CompareDepth(uv + float2(pixelSize.x, 0), depthCenter);
        outline += CompareDepth(uv + float2(0, -pixelSize.y), depthCenter);
        outline += CompareDepth(uv + float2(0, pixelSize.y), depthCenter);

        outline += CompareNormal(uv + float2(-pixelSize.x, 0), normalCenter);
        outline += CompareNormal(uv + float2(pixelSize.x, 0), normalCenter);
        outline += CompareNormal(uv + float2(0, -pixelSize.y), normalCenter);
        outline += CompareNormal(uv + float2(0, pixelSize.y), normalCenter);

        outline = saturate(outline / 8.0);

        return float4(_EdgeColor.rgb, outline * _EdgeColor.a);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "SilhouetteOutline"
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            HLSLPROGRAM
            ENDHLSL
        }
    }
}*/

