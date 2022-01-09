Shader "Holo/NormalizeShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "red" {}
        _Min ("Min", float) = 0
        _Max ("Max", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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
            float4 _MainTex_ST;

            float _Min;
            float _Max;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float frag (v2f i) : SV_Target
            {
                float col = tex2D(_MainTex, i.uv); // Sample
                col = (col - _Min) / (_Max - _Min); // Normalize
                // col = 1 - col; // Invert
                return col;
            }
            ENDCG
        }
    }
}
