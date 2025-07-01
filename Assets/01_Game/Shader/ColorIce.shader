Shader "URP/RevealByPlayerDistanceWithPattern"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _NearDistance ("Fully Visible Distance", Float) = 2.0
        _FarDistance ("Fully Invisible Distance", Float) = 6.0
        [HideInInspector] _PlayerPosition ("Player Position", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            float4 _BaseColor;
            float3 _PlayerPosition;
            float _NearDistance;
            float _FarDistance;

            sampler2D _BaseMap;
            float4 _BaseMap_ST;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionWS = positionWS;
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float dist = distance(_PlayerPosition, IN.positionWS);
                float fade = saturate((_FarDistance - dist) / (_FarDistance - _NearDistance));

                float4 texColor = tex2D(_BaseMap, IN.uv);
                float4 finalColor = texColor * _BaseColor;
                finalColor.a *= fade;
                return finalColor;
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
