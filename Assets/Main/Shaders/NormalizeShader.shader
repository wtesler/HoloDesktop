Shader "Holo/NormalizeShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "red" {}
        _Min ("Min", float) = 0
        _Diff ("Diff", float) = 0
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
            // float _MainTex_ST;

            float _Min;
            float _Diff;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float frag (v2f i) : SV_Target
            {
                return (tex2D(_MainTex, i.uv) - _Min) / _Diff; // Normalize
            }
            ENDCG
        }
    }
}
