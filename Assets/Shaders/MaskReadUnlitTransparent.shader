// Shader used by unlit transparent objects that need to be rendered only when seen through the mirror.
Shader "Custom/MaskReadUnlitTransparent"
{
    Properties
    {
        _Color ("Color", Color) = (0, 0, 0, 1)
        [IntRange] _StencilRef ("Stencil Reference Value", Range(0,255)) = 0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Stencil
        {
            Ref [_StencilRef]
            Comp Equal
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha 

        Pass {
            ZTest Always
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };
            float4 _Color;
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }
            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = i.color;
                return col * _Color;
            }
            ENDCG
        }
    }
}