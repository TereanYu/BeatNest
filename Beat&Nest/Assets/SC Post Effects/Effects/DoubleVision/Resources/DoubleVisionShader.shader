Shader "Hidden/SC Post Effects/Double Vision"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"
	#include "../../../Shaders/SCPE.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	float _Amount;

	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);

		screenColor += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo - float2(_Amount, 0));
		screenColor += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo + float2(_Amount, 0));

		return screenColor / 3.0;
	}

	float4 FragEdges(VaryingsDefault i) : SV_Target
	{
		float2 coords = 2.0 * i.texcoord - 1.0;
		float2 end = i.texcoord - coords * dot(coords, coords) * _Amount;
		float2 delta = (end - i.texcoord) / 3;

		half4 texelA = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, Distort(i.texcoordStereo), 0);
		half4 texelB = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, Distort(delta + i.texcoordStereo), 0);
		half4 texelC = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, Distort(delta * 2.0 + i.texcoordStereo), 0);

		half4 sum = (texelA + texelB + texelC) / 3.0;
		return sum;
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

		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragEdges

			ENDHLSL
		}
	}
}