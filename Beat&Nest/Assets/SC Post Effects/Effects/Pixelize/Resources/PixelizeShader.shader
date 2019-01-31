Shader "Hidden/SC Post Effects/Pixelize"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	float _Resolution;

	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float x = (int)(i.texcoordStereo.x / _Resolution) * _Resolution;
		float y = (int)(i.texcoordStereo.y / _Resolution) * _Resolution;

		return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, half2(x, y));
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