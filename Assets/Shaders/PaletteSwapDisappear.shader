﻿Shader "Custom/TextureSwapDisappear"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _SwapTex ("Swap Texture", 2D) = "white" {}
        _SwapProgress ("Swap Progress", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off 
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _SwapTex;
            float _SwapProgress;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 originalColor = tex2D(_MainTex, i.uv);
                fixed4 swapColor = tex2D(_SwapTex, i.uv);

                // If above the swap progress threshold, keep the original color
                if (i.uv.y < (1.0 - _SwapProgress))
                {
                    return originalColor;
                }

                // Otherwise, transition to the swap texture
                return swapColor;
            }
            ENDCG
        }
    }
}
