Shader "Hidden/SC Post Effects/Scanlines"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	float4 _Params;
	//X: Amount
	//Y: Intensity
	//Z: Speed

	float4 Frag(VaryingsDefault i): SV_Target
	{
		float2 uv = i.texcoordStereo;

		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

		half linesY = uv.y - sin(uv.y * _Params.x + (_Time.w * _Params.z)) * _Params.x;

		float3 color = lerp(screenColor, screenColor * linesY, _Params.y).rgb;

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