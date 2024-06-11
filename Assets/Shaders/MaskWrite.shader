// Shader used by the mirror surface to mask mirror only objects.
Shader "Custom/MaskWrite"
{
    Properties
    {
        [IntRange] _StencilRef ("Stencil Reference Value", Range(0,255)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "Queue"="Geometry"
        }

        Stencil
        {
            Ref [_StencilRef]
            Comp Always
            Pass Replace
        }

        Pass
        {
            Blend Zero One
            ZWrite Off

            CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2_f
            {
                float4 position : SV_POSITION;
            };

            v2_f vert(const appdata v)
            {
                v2_f o;
                o.position = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2_f _) : SV_TARGET
            {
                return 0;
            }
            ENDCG
        }
    }
}