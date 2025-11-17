Shader "TwoBitMachines/TransitionFade"
{
        Properties
        {
                _MainTex("Texture", 2D) = "white" {}
                _Color("Color", Color) = (1,1,1,1)
                _CutOff("Fade", Range(-0.1, 1.1)) = 0
        }

        SubShader
        {
                Tags { "Queue" = "Transparent" }
                Cull Off
                Lighting Off
                ZWrite Off
                Blend SrcAlpha OneMinusSrcAlpha

                Pass
                {
                        CGPROGRAM
                        #pragma vertex vert
                        #pragma fragment frag
                        #include "UnityCG.cginc"

                        struct appdata
                        {
                                float4 vertex : POSITION;
                                fixed4 color    : COLOR;
                                float2 uv : TEXCOORD0;
                        };

                        struct v2f
                        {
                                float2 uv : TEXCOORD0;
                                fixed4 color    : COLOR;
                                float4 vertex : SV_POSITION;
                        };

                        sampler2D _MainTex;
                        float _CutOff;
                        fixed4 _Color;

                        v2f vert(appdata v)
                        {
                                v2f o;
                                o.vertex = UnityObjectToClipPos(v.vertex);
                                o.uv = v.uv;
                                o.color = v.color *_Color;
                                return o;
                        }

                        fixed4 frag(v2f i) : SV_Target
                        {
                                fixed4 c = tex2D(_MainTex,i.uv) * i.color;
                                c.a = _CutOff;
                                return c;
                        }					
                        ENDCG
                }
        }
}
