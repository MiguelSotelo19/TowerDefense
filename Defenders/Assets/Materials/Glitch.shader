Shader "Custom/Glitch"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _GlitchAmount("Glitch Amount", Range(0,1)) = 0.2
        _RGBSplit("RGB Split", Range(0,0.1)) = 0.02
        _BlockSize("Block Size", Range(1,200)) = 50
        _Speed("Speed", Range(0,10)) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _GlitchAmount;
            float _RGBSplit;
            float _BlockSize;
            float _Speed;

            // NUEVA estructura (correcta para Unity 2021+ / Unity 6)
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // random simple
            float random(float2 st)
            {
                return frac(sin(dot(st, float2(12.9898,78.233))) * 43758.5453);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Bloque tipo glitch
                float2 blockUV = floor(uv * _BlockSize) / _BlockSize;
                float glitch = random(blockUV + _Time.y * _Speed) * _GlitchAmount;

                uv += glitch * 0.05;

                // RGB Split
                float2 rUV = uv + float2(_RGBSplit, 0);
                float2 gUV = uv + float2(0, _RGBSplit);
                float2 bUV = uv - float2(_RGBSplit, 0);

                float r = tex2D(_MainTex, rUV).r;
                float g = tex2D(_MainTex, gUV).g;
                float b = tex2D(_MainTex, bUV).b;

                return float4(r, g, b, 1);
            }
            ENDCG
        }
    }
}
