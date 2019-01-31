Shader "Hidden/SC Post Effects/Colorize"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"
	#include "../../../Shaders/Blending.hlsl"
	#include "../../../Shaders/SCPE.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_ColorRamp, sampler_ColorRamp);

	float _Intensity;
	half _BlendMode;

	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);

		half luminance = (screenColor.r * 0.3 + screenColor.g * 0.59 + screenColor.b * 0.11);

		float4 colors = SAMPLE_TEXTURE2D(_ColorRamp, sampler_ColorRamp, float2(luminance, 0));
	
		float3 color = 0;

		if (_BlendMode == 0) color = lerp(screenColor.rgb, colors.rgb, colors.a * _Intensity);
		if (_BlendMode == 1) color = lerp(screenColor.rgb, BlendAdditive(colors.rgb, screenColor.rgb), colors.a * _Intensity);
		if (_BlendMode == 2) color = lerp(screenColor.rgb, colors.rgb * screenColor.rgb, _Intensity);
		if (_BlendMode == 3) color = lerp(screenColor.rgb, BlendScreen(colors.rgb, screenColor.rgb), colors.a * _Intensity);

		return float4(color.rgb, screenColor.a);
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