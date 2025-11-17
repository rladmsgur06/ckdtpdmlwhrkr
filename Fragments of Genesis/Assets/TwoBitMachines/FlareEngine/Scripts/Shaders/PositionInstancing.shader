Shader "TwoBitMachines/PositionInstancing" 
{
            Properties
            {
                        _MainTex("Texture", 2D) = "white" {}
                        _Transparent("Transparent", Float) = 1
            }

            SubShader 
            {
                        Tags {"Queue" = "Transparent"}
                        Cull Off
                        Lighting Off
                        ZWrite Off
                        Blend SrcAlpha OneMinusSrcAlpha

                        Pass 
                        {
                                    CGPROGRAM
                                    
                                    #include "UnityCG.cginc"
                                    #pragma vertex vertexShader
                                    #pragma fragment fragmentShader
                                    #pragma multi_compile_instancing
                                    #pragma instancing_options procedural:setup

                                    struct vertexInput 
                                    {
                                                float4 vertex : POSITION;
                                                float2 uv	: TEXCOORD0;
                                                float4 color    : COLOR;
                                                UNITY_VERTEX_INPUT_INSTANCE_ID
                                    };


                                    struct vertexOutput 
                                    {
                                                float4 vertex : SV_POSITION;
                                                float2 uv	: TEXCOORD0;
                                                float4 color    : COLOR;
                                                UNITY_VERTEX_INPUT_INSTANCE_ID
                                    };


                                    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                                                float2 positionBuffer[1023];
                                                float2 directionBuffer[1023];
                                    #endif
                                    float4 colors[1023];
                                    
                                    void setup() 
                                    {

                                                #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                                                            float2 position = positionBuffer[unity_InstanceID];
                                                            float2 direction = directionBuffer[unity_InstanceID];

                                                            unity_ObjectToWorld = float4x4
                                                            (
                                                            direction.x, -direction.y, 0, position.x,
                                                            direction.y, direction.x, 0, position.y,
                                                            0, 0, 1, 0,
                                                            0, 0, 0, 1
                                                            );

                                                #endif
                                    }

                                    sampler2D _MainTex;
                                    float _Transparent;

                                    
                                    vertexOutput vertexShader(vertexInput v) 
                                    {
                                                vertexOutput o;
                                                UNITY_SETUP_INSTANCE_ID(v);
                                                o.vertex = UnityObjectToClipPos(v.vertex);
                                                o.uv = v.uv;
                                                o.color = float4(1, 1, 1, 1);
                                                #ifdef UNITY_INSTANCING_ENABLED
                                                            o.color = colors[unity_InstanceID];
                                                #endif
                                                return o;
                                    }

                                    float4 fragmentShader(vertexOutput o) : SV_Target
                                    {
                                                float4 c = tex2D(_MainTex, o.uv) * o.color;
                                                c.a *= _Transparent; 
                                                return c;
                                    }
                                    ENDCG
                        }
            }
            Fallback"Sprites/Default"
}
