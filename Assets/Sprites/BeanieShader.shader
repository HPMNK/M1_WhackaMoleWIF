Shader "Custom/Unlit/BeanieShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
_MaskTex("Mask Texture", 2D) = "white" { }
_Color("Color", Color) = (1, 0, 0, 1) // Rouge plein avec opacité complète
        _BlendMode("Blend Mode", Float) = 0 // Mode de fusion par défaut
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Blend SrcAlpha OneMinusSrcAlpha // Mode de mélange standard pour la transparence
        ZWrite Off
        Cull Off
        Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
#pragma fragment frag
# include "UnityCG.cginc"

            struct appdata
{
    float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

struct v2f
{
    float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

sampler2D _MainTex;
sampler2D _MaskTex;
float4 _Color;
float _BlendMode;

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    return o;
}

fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
fixed4 mask = tex2D(_MaskTex, i.uv);

// Vérifier si le pixel est exactement rouge (#FF0000)
if (mask.r > 0.9 && mask.g < 0.1 && mask.b < 0.1)
{
    // Appliquer la couleur en tenant compte de l'alpha
    fixed4 colorOverlay = _Color * mask.a;

    // Choisir le mode de fusion
    if (_BlendMode == 1) // Mode Multiply
    {
        col.rgb *= colorOverlay.rgb;
    }
    else if (_BlendMode == 2) // Mode Additive
    {
        col.rgb += colorOverlay.rgb;
    }
    else // Mode Default (Lerp)
    {
        col.rgb = lerp(col.rgb, colorOverlay.rgb, colorOverlay.a);
    }
}

return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
