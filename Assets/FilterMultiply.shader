Shader "Custom/ColorBlendFilter"
{
    Properties
    {
        _Color ("Color Tint", Color) = (1, 1, 1, 1)
        _BlendMode("Blend Mode", Int) = 0 // 0 = Additive, 1 = Multiply, 2 = Overlay
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
#pragma fragment frag
# include "UnityCG.cginc"

            struct appdata_t
{
    float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

struct v2f
{
    float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

float4 _Color;
int _BlendMode;

v2f vert(appdata_t v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.texcoord;
    return o;
}

fixed4 OverlayBlend(float4 baseColor, float4 blendColor)
{
    float3 result;
    for (int i = 0; i < 3; i++)
    {
        result[i] = baseColor[i] < 0.5 ? (2.0 * baseColor[i] * blendColor[i]) : (1.0 - 2.0 * (1.0 - baseColor[i]) * (1.0 - blendColor[i]));
    }
    return fixed4(result, baseColor.a);
}

fixed4 frag(v2f i) : SV_Target
            {
                fixed4 baseColor = float4(1, 1, 1, 1); // White base, assuming full visibility of the game behind
fixed4 finalColor = _Color;

if (_BlendMode == 0) // Additive
{
    finalColor.rgb = baseColor.rgb + _Color.rgb * _Color.a;
}
else if (_BlendMode == 1) // Multiply
{
    finalColor.rgb = baseColor.rgb * _Color.rgb;
    finalColor.a = baseColor.a * _Color.a;
}
else if (_BlendMode == 2) // Overlay
{
    finalColor = OverlayBlend(baseColor, _Color);
    finalColor.a = baseColor.a * _Color.a;
}

finalColor.rgb = lerp(baseColor.rgb, finalColor.rgb, _Color.a);
return finalColor;
            }
            ENDCG
        }
    }
    FallBack Off
}
