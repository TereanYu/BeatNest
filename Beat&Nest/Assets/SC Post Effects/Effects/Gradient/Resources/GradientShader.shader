Shader "Hidden/SC Post Effects/Gradient"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"
	#include "../../../Shaders/Blending.hlsl"
	#include "../../../Shaders/SCPE.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_Gradient, sampler_Gradient);
	float _Intensity;
	float _Rotation;
	float4 _Color1;
	float4 _Color2;
	half _BlendMode;

	inline float3 BlendColors(float3 colors, float3 screenColor, float alpha) 
	{
		float3 color = 0;

		if (_BlendMode == 0) color = lerp(screenColor, colors, alpha * _Intensity);
		if (_BlendMode == 1) color = lerp(screenColor, BlendAdditive(colors, screenColor), alpha * _Intensity);
		if (_BlendMode == 2) color = lerp(screenColor, colors * screenColor, _Intensity);
		if (_BlendMode == 3) color = lerp(screenColor, BlendScreen(colors, screenColor), alpha * _Intensity);

		return color;
	}

	float4 FragColors(VaryingsDefault i): SV_Target
	{
		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);	
		float2 gradientUV = RotateUV(i.texcoordStereo, _Rotation);

		float4 colors = lerp(_Color2, _Color1, gradientUV.y);

		float3 color = BlendColors(colors.rgb, screenColor.rgb, _Color1.a * _Color2.a);

		return float4(color.rgb, screenColor.a);
	}

	float4 FragTexture(VaryingsDefault i): SV_Target
	{
		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);
		float2 gradientUV = RotateUV(i.texcoordStereo, _Rotation);

		float4 gradient = SAMPLE_TEXTURE2D(_Gradient, sampler_Gradient, gradientUV);

		float3 color = BlendColors(gradient.rgb, screenColor.rgb, gradient.a);

		return float4(color.rgb, screenColor.a);
	}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		//Normal
		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragColors

			ENDHLSL
		}
		//Overlay
		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragTexture

			ENDHLSL
		}
	}
}