/**
* Spatial AR Unity Framework ~ The Crazy Bruce Lab
*
* Copyright (c) 2017 Andrew Irlitti.
*               2017 Daniel Jackson.
*
* This code can be used by members of the Wearable Computer Lab, University
* of South Australia for research purposes. Commercial use is not allowed
* without written permission. This copyright notice is subject to change.
*
*/

// Wireframe shader, uses barycentric coordinates derived from vertex color
//	channel to determing fragment distance from edges. If fragment is close
//	to an edge, apply WireColor, else apply FillColor.
//	Colors can be transparent, back faces are drawn in one pass, then front
//	faces are drawn later.

// Since depth sorting for transparent objects in Unity is done by object not
//	by triangle, there will be depth artifacting in concave objects.

// If back faces are closer to the camera than front faces, then transparency
//	will not render correctly. This may be due to incorrect vertex winding or
//	a projection matrix with an inverted forward vector.

Shader "Transparent/WireShader"
{
	Properties
	{
		_WireColorFront ("Wireframe Color (front face)", Color) = (0, 0, 0, 1)
		_FillColorFront ("Fill Color (front face)", Color) = (1, 1, 1, 1)
		_WireColorBack ("Wireframe Color (back face)", Color) = (0, 0, 0, 1)
		_FillColorBack ("Fill Color (back face)", Color) = (1, 1, 1, 1)
		_LineWeight ("Line Weight", Float) = 3.0
	}

	CGINCLUDE // CG code included in all passes.
	#include "UnityCG.cginc" // Unity macros

	fixed4 _WireColorFront;
	fixed4 _FillColorFront;
	fixed4 _WireColorBack;
	fixed4 _FillColorBack;
	float _LineWeight;

	struct appdata
	{
		float4 vertex : POSITION;
		float3 color : COLOR;
	};

	struct v2f
	{
		UNITY_FOG_COORDS(1) 
		float4 vertex : SV_POSITION;
		float3 color : COLOR;
	};

	float edgeFactor(v2f i) {
		// Get partial derivative of barycentric coord. Gives a factor to correct for 
		//	forshorting of lines when tris are not co-planar with camera.
		float3 d = fwidth(i.color);
		// Interpolate based on barycentric coords. Gets distance of fragment from each edge
		//	of the tri in screenspace.
		float3 a3 = smoothstep(float3(0, 0, 0), d * _LineWeight, i.color);
		// Take the minimum value as the line weight at the frag position.
		return min(min(a3.x, a3.y), a3.z);
	}

	// HLSL vert shader
	v2f vert (appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		UNITY_TRANSFER_FOG(o,o.vertex);
		o.color = v.color;
		return o;
	}

	ENDCG

	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		// Pass twice, painters algorithm draw only back faces, 
		//	then only front faces.
		Pass
		{
			Cull Front // Back faces only
			CGPROGRAM

			fixed4 WireColor() { return _WireColorBack; }
			fixed4 FillColor() { return _FillColorBack; }

			// HLSL frag shader
			fixed4 frag(v2f i) : SV_Target
			{
				// Color = LERP based on proximity to edge.
				fixed4 col = lerp(WireColor(), FillColor(), edgeFactor(i));
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}

			#pragma vertex vert // HLSL vert & frag shader
			#pragma fragment frag
			#pragma multi_compile_fog // make fog work
			ENDCG
		}

		Pass
		{
			Cull Back // Front faces
			CGPROGRAM

			fixed4 WireColor() { return _WireColorFront; }
			fixed4 FillColor() { return _FillColorFront; }

			// HLSL frag shader
			fixed4 frag(v2f i) : SV_Target
			{
				// Color = LERP based on proximity to edge.
				fixed4 col = lerp(WireColor(), FillColor(), edgeFactor(i));
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}

			#pragma vertex vert // HLSL vert & frag shader
			#pragma fragment frag
			#pragma multi_compile_fog // make fog work
			ENDCG
		}
	}
}
