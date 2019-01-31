Shader "Hidden/SC Post Effects/Sharpen"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	float _Amount;

	float4 Frag(VaryingsDefault i): SV_Target
	{
		float2 uv = i.texcoordStereo;

		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

		float2 sampleDistance = 1.0 / _ScreenParams.xy;
		float3 sampleTL = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( - sampleDistance.x, - sampleDistance.y) * 1.5).rgb;
		float3 sampleTR = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(sampleDistance.x, - sampleDistance.y) * 1.5).rgb;
		float3 sampleBL = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( - sampleDistance.x, sampleDistance.y) * 1.5).rgb;
		float3 sampleBR = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(sampleDistance.x, sampleDistance.y) * 1.5).rgb;

		float3 offsetColors = 0.25 * (sampleTL + sampleTR + sampleBL + sampleBR);

		float3 sharpenedColor = screenColor.rgb + (screenColor.rgb - offsetColors) * _Amount;

		return float4(sharpenedColor.rgb, screenColor.a);
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