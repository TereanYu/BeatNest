Shader "Hidden/SC Post Effects/Posterize"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"
	#include "../../../Shaders/Colors.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	float _Depth;

	#pragma multi_compile _ UNITY_COLORSPACE_GAMMA

	float4 Frag(VaryingsDefault i): SV_Target
	{
		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);
		float bits = _Depth  * (0.5 + 0.5);	
		float k = pow(2, bits);

		screenColor = floor(screenColor * k + 0.5) /k;

#if UNITY_COLORSPACE_GAMMA
		screenColor = LinearToSRGB(screenColor);
#endif

		return screenColor;
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