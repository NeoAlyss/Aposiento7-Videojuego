Shader "UniversalPlatform/UI/Grayscale"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _GrayscaleAmount ("Grayscale Amount", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t { float4 vertex : POSITION; float2 texcoord : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; half2 texcoord : TEXCOORD0; fixed4 color : COLOR; };

            sampler2D _MainTex;
            fixed4 _Color;
            float _GrayscaleAmount;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = tex2D(_MainTex, IN.texcoord) * IN.color;
                float gray = dot(color.rgb, float3(0.299, 0.587, 0.114));
                color.rgb = lerp(color.rgb, float3(gray, gray, gray), _GrayscaleAmount);
                return color;
            }
            ENDCG
        }
    }
}
