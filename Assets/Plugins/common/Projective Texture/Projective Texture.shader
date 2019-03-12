/**
* Spatial AR Unity Framework ~ The Crazy Bruce Lab
*
* Copyright (c) 2018 Daniel Jackson.
*
* This code can be used by members of the Wearable Computer Lab, University
* of South Australia for research purposes. Commercial use is not allowed
* without written permission. This copyright notice is subject to change.
*/
/*
	Shader applies a texture to a surface as if projected onto it. Requires
	a View/Projection matrix, as if from a pinhole camera, this models the
	intrinsic and extrinsic properties of the projector.
	Known Bugs: No mechanism to check for face occlusion or for projection 
	onto back faces at this time.
*/

Shader "Hidden/Projective Texture" {
	Properties {
		_MainTex("Texture", 2D) = "white" {}
	}
	SubShader{
		Tags {"RenderType" = "Opaque" }
		CGPROGRAM
		#pragma surface surf Lambert
		struct Input {
			float3 worldPos; // Position of the fragment in world space.
		};
		sampler2D _MainTex; // Image to project
		float4x4 _Projector_VP; // Projection * Camera Transform Matrix
		void surf(Input IN, inout SurfaceOutput o)
		{
			// Get position of fragment in projectors screen space.
			float4 pos_screen = mul(_Projector_VP, float4(IN.worldPos, 1.0));
			// Normalize and perpective transform.
			pos_screen *= 1 / pos_screen.w; 
			pos_screen = (1 + pos_screen) / 2;
			// Clip to projector frustrum
			clip(pos_screen.xyz); clip(1 - pos_screen.xyz);
			// Output texture to Albedo channel
			o.Albedo = tex2D(_MainTex, pos_screen.xy).rgb; 
		}
		ENDCG
	}
	Fallback "Diffuse"
}
