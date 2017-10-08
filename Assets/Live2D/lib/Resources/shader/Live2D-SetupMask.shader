Shader "Live2D/SetupMask" {
	Properties { 
		_MainTex ("Base (RGB)", 2D) = "white" {} 
	}

	CGINCLUDE
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members mypos)
// #pragma exclude_renderers d3d11 xbox360

	#pragma vertex vert 
	#pragma fragment frag
	#include "UnityCG.cginc"

	sampler2D _MainTex;
	float4   _ChannelFlag; // Color Channel Flag 
	float4x4 _MaskMatrix ; 
	float4   _Bounds ; // Bounds (x,y,right,bottom)

#if ! defined( SV_Target )
	#define SV_Target	COLOR
#endif

#if ! defined( SV_POSITION )
	#define SV_POSITION	POSITION
#endif

	 struct v2f
	 {
		float4 position : POSITION;
		float4 mypos : TEXCOORD1;
		float2 texcoord : TEXCOORD0;
		float4 color:COLOR0;
	 };


	 v2f vert(float4 vertexPos : POSITION,float4 texcoord : TEXCOORD0,float4 color:COLOR)
	 {
		v2f OUT;
		OUT.position = mul(_MaskMatrix, vertexPos);

		OUT.mypos = OUT.position;
		OUT.texcoord=texcoord;
		OUT.color=color;
		return OUT;
	 }

	ENDCG
	
	
	SubShader {
		Tags { "Queue" = "Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100
		BindChannels{ Bind "Vertex", vertex Bind "texcoord", texcoord Bind "Color", color }


		//0 : draw 1 channel 
		Pass {
			// C = Cs*0 + Cd*(1-Cs)
			Blend Zero OneMinusSrcColor , Zero OneMinusSrcAlpha ZWrite Off Lighting Off Cull Off
			CGPROGRAM // here begins the part in Unity's Cg

			void frag(v2f IN, out float4 color:COLOR) // fragment shader
			{
				float isInside = 
				      step( _Bounds.x , IN.mypos.x/IN.mypos.w ) 
					* step( _Bounds.y , IN.mypos.y/IN.mypos.w ) 
					* step(IN.mypos.x/IN.mypos.w , _Bounds.z )
					* step(IN.mypos.y/IN.mypos.w , _Bounds.w ) ;// 範囲外なら 0 、範囲内なら 1 

				color = _ChannelFlag * tex2D (_MainTex, IN.texcoord).a * isInside ;
			}

			ENDCG // here ends the part in Cg 
		}
	}
}
