Shader "Custom/TrainerShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _PaletteTex ("Palette Texture", 2D) = "white" {}
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
            sampler2D _PaletteTex;
            float _SwapProgress; // 0 = original, 1 = full palette swap

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

                // Swap from TOP to BOTTOM
                if (i.uv.y < (1.0 - _SwapProgress))
                {
                    return originalColor; // Keep original color above the threshold
                }

                // Convert original color to grayscale for palette lookup
                float grayscale = dot(originalColor.rgb, float3(0.299, 0.587, 0.114));

                // Map grayscale to 5 discrete colors
                float index = floor(grayscale * 5.0) / 5.0;

                // Sample the palette using the index
                fixed4 paletteColor = tex2D(_PaletteTex, float2(index, 0.5));

                return fixed4(paletteColor.rgb, originalColor.a);
            }
            ENDCG
        }
    }
}