Shader "RPSKnights/Unlit"
{
    Properties { _Color ("Color", Color) = (1,1,1,1) }
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
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f { float4 vertex : SV_POSITION; float3 normal : TEXCOORD0; };
            v2f vert(appdata value)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(value.vertex);
                output.normal = UnityObjectToWorldNormal(value.normal);
                return output;
            }
            fixed4 frag(v2f input) : SV_Target
            {
                float3 lightDirection = normalize(float3(-0.35, 0.7, -0.6));
                float brightness = 0.55 + 0.45 * saturate(dot(normalize(input.normal), lightDirection));
                return fixed4(_Color.rgb * brightness, _Color.a);
            }
            ENDCG
        }
    }
}
