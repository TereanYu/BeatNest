Shader "Hidden/SC Post Effects/Dithering"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

	float4 _Dithering_Coords;
	//X: Size
	//Y: (Size)
	//Z: Luminance influence
	//W: Intensity

	// Screen-space 4x4 Bayer matrix, based on https://en.wikipedia.org/wiki/Ordered_dithering
	float BinaryDither4x4(float value, float2 screenPos) {
		float4x4 mtx = float4x4(
			float4(1, 9, 3, 11) / 17.0,
			float4(13, 5, 15, 7) / 17.0,
			float4(4, 12, 2, 10) / 17.0,
			float4(16, 8, 14, 6) / 17.0
			);
		float2 px = floor(_ScreenParams.xy * screenPos);
		int xSmp = fmod(px.x, 4);
		int ySmp = fmod(px.y, 4);
		float4 xVec = 1 - saturate(abs(float4(0, 1, 2, 3) - xSmp));
		float4 yVec = 1 - saturate(abs(float4(0, 1, 2, 3) - ySmp));
		float4 pxMult = float4(dot(mtx[0], yVec), dot(mtx[1], yVec), dot(mtx[2], yVec), dot(mtx[3], yVec));
		return round(value + dot(pxMult, xVec));
	}

	float4 Frag(VaryingsDefault i) : SV_Target
	{

		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);

		float luminance = (screenColor.r * 0.3 + screenColor.g * 0.59 + screenColor.b * 0.11);

		float dither = BinaryDither4x4(luminance * _Dithering_Coords.z, i.texcoordStereo * _Dithering_Coords.x);

		return lerp(screenColor, screenColor * dither, _Dithering_Coords.w);

	}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment Frag

			ENDHLSL
		}
	}
}