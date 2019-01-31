Shader "Hidden/SC Post Effects/SpeedLines"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_NoiseTex, sampler_NoiseTex);
	float4 _Params;

	float2 CartToPolar(float2 uv) {
		float2 polar = uv - float2(0.5, 0.5);
		float2 uv2 = polar;

		//Radial
		polar.x = length(polar) *0.01;
		//Angular
		polar.y = 0.5 + (atan2(uv2.x, uv2.y) / 6.283185);

		polar.y *= _Params.z;
		polar.y += frac(_Time.w);

		return polar;
	}

	float RadialMask(float2 uv) {
		float falloff = length(float2(float2(0.5, 0.5) - frac(uv)) / 0.70);
		falloff = pow(falloff, _Params.y);
		falloff = saturate(falloff);

		return falloff;
	}

	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float2 uv = CartToPolar(i.texcoord);
		//return float4(uv.x, uv.y, 0, 0);
		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);

		float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv).r;

		noise *= RadialMask(i.texcoord);
		float3 color = lerp(screenColor.rgb, screenColor.rgb + noise, _Params.x);

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