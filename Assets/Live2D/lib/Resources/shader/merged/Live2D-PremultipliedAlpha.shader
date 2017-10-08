// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Live2D/Live2D PremultipliedAlpha" {
	Properties{
		_Cull("Culling", Int) = 0
		_SrcColor("SrcColor", Int) = 0
		_DstColor("DstColor", Int) = 0
		_SrcAlpha("SrcAlpha", Int) = 0
		_DstAlpha("DstAlpha", Int) = 0
	}
		
	CGINCLUDE
	#pragma vertex vert 
	#pragma fragment frag
	#include "UnityCG.cginc"

	sampler2D _MainTex;

#if ! defined( SV_Target )
	#define SV_Target	COLOR
#endif

#if ! defined( SV_POSITION )
	#define SV_POSITION	POSITION
#endif

	struct appdata_t {
		float4 vertex : POSITION;
		fixed4 color : COLOR;
		float2 texcoord : TEXCOORD0;
	};
			
	struct v2f {
		float4 position : SV_POSITION;
		float2 texcoord : TEXCOORD0;
		float4 color:COLOR;
	};

	ENDCG
				
	SubShader {
		Tags { "Queue" = "Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100
		BindChannels{ Bind "Vertex", vertex Bind "texcoord", texcoord Bind "Color", color }

		Pass {
			Blend[_SrcColor][_DstColor],[_SrcAlpha][_DstAlpha]
			ZWrite Off
			Lighting Off
			Cull[_Cull]

			CGPROGRAM

			
			v2f vert(appdata_t v)
			{
				v2f OUT;
				OUT.position = UnityObjectToClipPos(v.vertex);
				OUT.texcoord = v.texcoord ;
				OUT.color=v.color;
				return OUT;
			}
				
							
			float4 frag ( v2f IN) : SV_Target
			{
				float4 tex = tex2D (_MainTex, IN.texcoord) * IN.color ;
				return tex;
			}
				
			ENDCG
		}
	}
}