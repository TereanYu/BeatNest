Shader "Hidden/SC Post Effects/Black Bars"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

	float4 _MainTex_TexelSize;
	float2 _Size;

	float4 FragHorizontal(VaryingsDefault i): SV_Target
	{
		float2 uv = i.texcoord;
		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);

		half bars = min(uv.y, (1-uv.y));
		bars = step(_Size.x * _Size.y, bars);

		return float4(screenColor.rgb * bars, screenColor.a);
	}

	float4 FragVertical(VaryingsDefault i) : SV_Target
	{
		float2 uv = i.texcoord;
		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);

		half bars = (uv.x * (1-uv.x));
		bars = step(_Size.x * (_Size.y /2), bars);

		return float4(screenColor.rgb * bars, screenColor.a);
	}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragHorizontal

			ENDHLSL
		}
		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragVertical

			ENDHLSL
		}
	}
}