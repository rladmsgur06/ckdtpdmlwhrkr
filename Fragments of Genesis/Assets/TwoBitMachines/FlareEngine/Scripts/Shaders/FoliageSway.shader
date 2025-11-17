Shader "TwoBitMachines/FoliageSway"
{
        Properties
        {
                [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

                _Amplitude ("Amplitude", Range(0, 2)) = 0.1
                _Frequency ("Frequency", Range(0, 10)) = 1
                _Speed ("Speed", Range(0, 10)) = 1
                _Direction ("Direction", Float) = 0

                _Color ("Tint", Color) = (1,1,1,1)
                [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        }

        SubShader
        {
                Tags
                { 
                        "Queue"="Transparent" 
                        "IgnoreProjector"="True" 
                        "RenderType"="Transparent" 
                        "PreviewType"="Plane"
                        "CanUseSpriteAtlas"="True"
                }

                Cull Off
                Lighting Off
                ZWrite Off
                Blend One OneMinusSrcAlpha

                Pass
                {
                        CGPROGRAM
                        #pragma vertex vert
                        #pragma fragment frag
                        #pragma multi_compile _ PIXELSNAP_ON
                        #include "UnityCG.cginc"
                        
                        struct appdata_t
                        {
                                float4 vertex   : POSITION;
                                float4 color    : COLOR;
                                float2 texcoord : TEXCOORD0;
                        };

                        struct v2f
                        {
                                float4 vertex   : SV_POSITION;
                                fixed4 color    : COLOR;
                                float2 texcoord  : TEXCOORD0;
                        };

                        float _Amplitude;
                        float _Frequency;
                        float _Speed;
                        float _Direction;
                        fixed4 _Color;

                        v2f vert(appdata_t IN)
                        {
                                v2f OUT;

                                float direction = _Direction;
                                if(direction == 0 && IN.vertex.y  > 0) //         bottom
                                {
                                        IN.vertex.x += sin(_Time.y * _Speed + IN.vertex.x * _Frequency) * _Amplitude;
                                }
                                else if(direction == 1 && IN.vertex.y <= 0) //    top
                                {
                                        IN.vertex.x += sin(_Time.y * _Speed + IN.vertex.x * _Frequency) * _Amplitude;
                                }
                                else if(direction == 2 && IN.vertex.x <= 0 ) //   left
                                {
                                        IN.vertex.y +=  sin(_Time.y * _Speed + IN.vertex.y * _Frequency) * _Amplitude;
                                }
                                else if(direction == 3 && IN.vertex.x > 0) //     right
                                {
                                        IN.vertex.y += sin(_Time.y * _Speed + IN.vertex.y * _Frequency) * _Amplitude;
                                }

                                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                                OUT.texcoord = IN.texcoord;
                                OUT.color = IN.color * _Color;
                                #ifdef PIXELSNAP_ON
                                        OUT.vertex = UnityPixelSnap (OUT.vertex);
                                #endif

                                return OUT;
                        }

                        sampler2D _MainTex;
                        sampler2D _AlphaTex;
                        float _AlphaSplitEnabled;

                        fixed4 SampleSpriteTexture (float2 uv)
                        {
                                fixed4 color = tex2D (_MainTex, uv);

                                #if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
                                        if (_AlphaSplitEnabled)
                                        color.a = tex2D (_AlphaTex, uv).r;
                                #endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

                                return color;
                        }

                        fixed4 frag(v2f IN) : SV_Target
                        {
                                fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
                                c.rgb *= c.a;
                                return c;
                        }
                        ENDCG
                }
        }
}