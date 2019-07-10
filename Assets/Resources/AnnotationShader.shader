Shader "Unlit/AnnotationShader"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "" {}
	}
	SubShader
	{
		Tags { "Queue" = "Overlay" }
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Always
			ZWrite Off
			Cull Off
			Fog { Mode Off }
			SetTexture [_MainTex] { combine texture }
		}
	}
}
