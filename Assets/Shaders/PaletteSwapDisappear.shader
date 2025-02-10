Shader "Custom/PaletteSwapDisappear"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _PaletteTex ("Palette Texture", 2D) = "white" {}
        _EnablePaletteSwap ("Enable Palette Swap", Float) = 0
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
            float _EnablePaletteSwap;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);

                // Check if palette swap is enabled
                if (_EnablePaletteSwap < 0.5)
                {
                    return color; // Normal sprite colors
                }

                // Convert to grayscale for palette indexing
                float grayscale = dot(color.rgb, float3(0.299, 0.587, 0.114));

                // Map grayscale to 5 discrete colors
                float index = floor(grayscale * 5.0) / 5.0;

                // Sample the palette using the index
                fixed4 paletteColor = tex2D(_PaletteTex, float2(index, 0.5));

                return fixed4(paletteColor.rgb, color.a);
            }
            ENDCG
        }
    }
}
