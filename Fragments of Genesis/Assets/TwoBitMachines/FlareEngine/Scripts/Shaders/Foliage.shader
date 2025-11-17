Shader "TwoBitMachines/Foliage"
{
            Properties
            {
                        _TextureIdx("Texture Idx", float) = 0.0
                        _Textures("Textures", 2DArray) = "" {}
                        _Offset("Offset", float) = 0
                        _Top("Top", float) = 0
                        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
            }
            
            SubShader
            {
                        Tags { "Queue" = "Transparent" }
                        Cull Off
                        Lighting Off
                        ZWrite Off
                        Blend SrcAlpha OneMinusSrcAlpha
                        
                        Pass{
                                    
                                    CGPROGRAM
                                    #pragma vertex vert
                                    #pragma fragment frag
                                    #pragma target 3.5
                                    #pragma multi_compile_instancing
                                    #include "UnityCG.cginc"
                                    
                                    UNITY_DECLARE_TEX2DARRAY(_Textures);
                                    
                                    struct appdata_t
                                    {
                                                float4 vertex   : POSITION;
                                                float3 texcoord : TEXCOORD0;
                                                UNITY_VERTEX_INPUT_INSTANCE_ID
                                    };

                                    struct v2f
                                    {
                                                float4 vertex   : SV_POSITION;
                                                float3 texcoord  : TEXCOORD0;
                                    };
                                    
                                    UNITY_INSTANCING_BUFFER_START(Props)
                                    UNITY_DEFINE_INSTANCED_PROP(float, _TextureIdx)
                                    UNITY_DEFINE_INSTANCED_PROP(float, _Offset)
                                    UNITY_DEFINE_INSTANCED_PROP(float, _Top)
                                    UNITY_INSTANCING_BUFFER_END(Props)

                                    v2f vert(appdata_t IN)
                                    {
                                                v2f OUT;
                                                UNITY_SETUP_INSTANCE_ID(IN);
                                                float direction = UNITY_ACCESS_INSTANCED_PROP(Props, _Top);
                                                if(direction == 0 && IN.vertex.y  > 0) //         bottom
                                                {
                                                            IN.vertex.x += UNITY_ACCESS_INSTANCED_PROP(Props, _Offset);
                                                }
                                                else if(direction == 1 && IN.vertex.y <= 0) //    top
                                                {
                                                            IN.vertex.x += UNITY_ACCESS_INSTANCED_PROP(Props, _Offset);
                                                }
                                                else if(direction == 2 && IN.vertex.x <= 0 ) //   left
                                                {
                                                            IN.vertex.y += UNITY_ACCESS_INSTANCED_PROP(Props, _Offset);
                                                }
                                                else if(direction == 3 && IN.vertex.x > 0) //     right
                                                {
                                                            IN.vertex.y += UNITY_ACCESS_INSTANCED_PROP(Props, _Offset);
                                                }
                                                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                                                OUT.texcoord = IN.texcoord;
                                                OUT.texcoord.z = UNITY_ACCESS_INSTANCED_PROP(Props, _TextureIdx);
                                                #ifdef PIXELSNAP_ON
                                                            OUT.vertex = UnityPixelSnap (OUT.vertex);
                                                #endif
                                                return OUT;
                                    }

                                    fixed4 frag(v2f IN) : SV_Target
                                    {
                                                fixed4 c = UNITY_SAMPLE_TEX2DARRAY(_Textures, IN.texcoord);
                                                c.rgb *= c.a;
                                                return c;
                                    }
                                    
                                    ENDCG
                        }
            }
            Fallback"Sprites/Default"
}


