Shader "Cowsins/Blink"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_SelfIllum("Self Illumination",Range(0.0,1.0)) = 0.0
		_FlashAmount("Flash Amount",Range(0.0,1.0)) = 0.0
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
				"ForceNoShadowCasting" = "False"
			}

			Cull Off
			Lighting Off
			ZWrite On
			Fog { Mode Off }
			Blend SrcAlpha OneMinusSrcAlpha

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile DUMMY PIXELSNAP_ON

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
				fixed4 _Color;
				float _FlashAmount,_SelfIllum;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.uv) * _Color;
					fixed4 finalColor = lerp(col, fixed4(1, 1, 1, 1), _FlashAmount);

					return finalColor * _SelfIllum;
				}
				ENDCG
			}

			Pass
			{
				Name "SHADOWCASTER"
				Tags { "LightMode" = "ShadowCaster" }

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile DUMMY PIXELSNAP_ON

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
				};

				struct v2f
				{
					float4 pos : POSITION;
				};

				v2f vert(appdata v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					return float4(1, 1, 1, 1);
				}
				ENDCG
			}
		}

			Fallback "Transparent/VertexLit"
}
