Shader "Hidden/SC Post Effects/Radial Blur"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	float _Amount;
	float _Iterations;
	#define SAMPLES_INT 6

	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float2 blurVector = (float2(0.5, 0.5) - i.texcoord.xy) * _Amount;

		half4 color = half4(0,0,0,0);
		[unroll(12)]
		for (int j = 0; j < _Iterations; j++)
		{
			half4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);
			color += screenColor;
			i.texcoordStereo.xy += blurVector;
		}

		return color / _Iterations;
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