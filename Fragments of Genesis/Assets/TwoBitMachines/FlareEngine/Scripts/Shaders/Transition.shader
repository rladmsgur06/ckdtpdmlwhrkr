Shader "TwoBitMachines/Transition"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Transition("Transition Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_CutOff("CutOff", Range(-0.1, 1.1)) = 0
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
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			float4 _MainTex_TexelSize;
			sampler2D _Transition;
			sampler2D _MainTex;
			float _CutOff;
			fixed4 _Color;
			fixed4 _Clear;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.uv1 = v.uv;

				#if UNITY_UV_STARTS_AT_TOP
					if (_MainTex_TexelSize.y < 0)
					o.uv1.y = 1 - o.uv1.y;
				#endif
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 transit = tex2D(_Transition, i.uv);

				if (transit.b < _CutOff)
				return _Color;

				return fixed4(0,0,0,0);
			}					
			ENDCG
		}
	}
}
