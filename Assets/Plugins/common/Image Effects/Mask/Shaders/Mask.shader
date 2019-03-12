/**
* Spatial AR Unity Framework ~ The Crazy Bruce Lab
*
* Copyright (c) 2018 Daniel Jackson.
*
* This code can be used by members of the Wearable Computer Lab, University
* of South Australia for research purposes. Commercial use is not allowed
* without written permission. This copyright notice is subject to change.
*/
Shader "Hidden/UnitySARCommon/Mask"
// Image effect shader. For each fragment, lerps from _ClearColor to _MainTex based on value of _Mask.
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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
				float4 vertex : SV_POSITION;
			};

			//v2f vert (appdata v)
			//{
			//	v2f o;
			//	o.vertex = UnityObjectToClipPos(v.vertex);
			//	o.uv = v.uv;
			//	return o;
			//}

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _Mask;
			fixed4 _ClearColor;

			//fixed4 frag(v2f i) : SV_Target
			//{
			//	// Main texture is rendered image.
			//	fixed4 col = tex2D(_MainTex, i.uv);
			//// Get mask value for fragment.
			//fixed4 mask = tex2D(_Mask, i.uv);
			//// Blend from clear color to sampled rendered color, based on mask value.
			//col = lerp(_ClearColor, col, mask.r);
			//return col;
			//}

				fixed4 frag(in float2 in_uv : TEXCOORD0) : SV_Target
			{
					// Main texture is rendered image.
					fixed4 col = tex2D(_MainTex, in_uv);
				// Get mask value for fragment.
				float weight = tex2D(_Mask, in_uv).r;
				// Blend from clear color to sampled rendered color, based on mask value.
				col = lerp(_ClearColor, col, weight);
				return col;
			}

			ENDCG
		}
	}
}
