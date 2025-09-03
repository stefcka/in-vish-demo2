Shader "02_Selection/Fullscreen"
{
    Properties
    {
        _SamplePrecision ("Sampling Precision", Range(1,3)) = 1
        _OutlineWidth    ("Outline Width", Float)         = 5
        _InnerColor      ("Inner Color", Color)           = (1,1,0,0.5)
        _OuterColor      ("Outer Color", Color)           = (1,1,0,1)
        _Texture         ("Texture", 2D)                  = "black" {}
        _TextureSize     ("Texture Size", Vector)         = (64,64,0,0)
        _BehindFactor    ("Behind Factor", Range(0,1))    = 0.2
    }

    SubShader
    {
        Pass
        {
            Name "Custom Pass 0"
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FullScreenPass
            #pragma target 4.5
            #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

            #define MAXSAMPLES 16
            static float2 offsets[MAXSAMPLES] = {
                float2( 1,  0), float2(-1,  0), float2( 0,  1), float2( 0, -1),
                float2(.7071,.7071), float2(.7071,-.7071), float2(-.7071,.7071), float2(-.7071,-.7071),
                float2(.9239,.3827), float2(.9239,-.3827), float2(-.9239,.3827), float2(-.9239,-.3827),
                float2(.3827,.9239), float2(.3827,-.9239), float2(-.3827,.9239), float2(-.3827,-.9239)
            };

            int    _SamplePrecision;
            float  _OutlineWidth;
            float4 _InnerColor;
            float4 _OuterColor;
            Texture2D _Texture; float2 _TextureSize;
            float  _BehindFactor;

            float4 FullScreenPass(Varyings varyings) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);

                // 1) Sample raw camera depth and linearize
                float camRaw = LoadCameraDepth(varyings.positionCS.xy);
                float camLin = LinearEyeDepth(camRaw, _ZBufferParams);

                // 2) Get object depth from mask buffer and linearize
                PositionInputs pos = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, camRaw, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
                float objRaw = LoadCustomDepth(pos.positionSS);
                float objLin = LinearEyeDepth(objRaw, _ZBufferParams);

                // 3) Early‐out if object is further than what the camera saw
                if (objLin > camLin + 0.01)    // tweak epsilon as needed
                    return float4(0,0,0,0);

                // 4) Sample center alpha for edge detection
                float4 center = LoadCustomColor(pos.positionSS);
                float  objA   = center.a;

                // 5) Build outline mask
                uint   count   = min(2 * pow(2, _SamplePrecision), MAXSAMPLES);
                float4 outline = float4(0,0,0,0);
                float2 uvOff   = 1.0 / _ScreenSize.xy;
                for (uint i = 0; i < count; ++i)
                {
                    float2 uv = pos.positionNDC + uvOff * _OutlineWidth * offsets[i];
                    outline = max(outline, SampleCustomColor(uv));
                }

                // 6) Compose outer and inner colors
                float4 outer = _OuterColor * float4(outline.rgb, 1);
                outer.a     *= outline.a;

                float4 inner = SAMPLE_TEXTURE2D(_Texture, s_trilinear_repeat_sampler, pos.positionSS / _TextureSize)
                               * _InnerColor;

                // 7) Blend outline vs. fill
                return lerp(outer, inner * float4(center.rgb,1), objA);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
