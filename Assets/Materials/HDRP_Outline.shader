Shader "Custom/HDRP_Outline"
{
    Properties
    {
        _Width ("Outline Width", Float) = 0.05
        _Color ("Outline Color", Color) = (1,1,0,1)
    }
    SubShader
    {
        Tags 
        { 
            "RenderPipeline"="HDRenderPipeline" 
            "Queue"="Geometry+1" 
            "RenderType"="Opaque" 
        }

        Pass
        {
            Name "Outline"
            Cull Off      // draw only backfaces of the inflated shell
            ZWrite Off      // don’t write depth so original geometry stays
            ZTest LEqual    // only draw where original geometry is visible
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"                  // brings in UNITY_MATRIX_VP, unity_ObjectToWorld, etc.

            float _Width;
            float4 _Color;

            struct app
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct v2f
            {
                float4 posCS : SV_POSITION;
            };

            v2f vert(app IN)
            {
                v2f OUT;
                // object-to-world
                float4 worldPos4 = mul(unity_ObjectToWorld, float4(IN.positionOS,1));
                float3 worldPos  = worldPos4.xyz;
                float3 worldN    = normalize(mul((float3x3)unity_ObjectToWorld, IN.normalOS));

                // inflate
                worldPos += worldN * _Width;

                // world-to-clip
                OUT.posCS = mul(UNITY_MATRIX_VP, float4(worldPos,1));
                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
{
    // Pure solid red, full opacity, every pixel
    return float4(1,0,0,1);
}

            ENDHLSL
        }
    }
}
