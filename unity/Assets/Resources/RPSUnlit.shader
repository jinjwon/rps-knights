Shader "RPSKnights/Unlit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _RimColor ("Rim Color", Color) = (0.5,0.8,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            fixed4 _RimColor;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f { float4 vertex : SV_POSITION; float3 normal : TEXCOORD0; float3 viewDir : TEXCOORD1; };
            v2f vert(appdata value)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(value.vertex);
                output.normal = UnityObjectToWorldNormal(value.normal);
                float3 worldPosition = mul(unity_ObjectToWorld, value.vertex).xyz;
                output.viewDir = _WorldSpaceCameraPos.xyz - worldPosition;
                return output;
            }
            fixed4 frag(v2f input) : SV_Target
            {
                float3 lightDirection = normalize(float3(-0.35, 0.7, -0.6));
                float3 normal = normalize(input.normal);
                float diffuse = saturate(dot(normal, lightDirection));
                float rim = pow(1.0 - saturate(dot(normal, normalize(input.viewDir))), 2.6);
                float highlight = pow(saturate(dot(normal, normalize(lightDirection + normalize(input.viewDir)))), 22.0);
                float3 lit = _Color.rgb * (0.38 + diffuse * 0.62);
                lit += _RimColor.rgb * rim * 0.32 + highlight * 0.22;
                return fixed4(lit, _Color.a);
            }
            ENDCG
        }
    }
}
