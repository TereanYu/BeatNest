Shader "Hidden/SC Post Effects/Danger"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_Overlay, sampler_Overlay);
	float4 _Color;
	float4 _Params;
	//X: Intensity
	//Y: Size


	float Vignette(float2 uv)
	{
		float vignette = uv.x * uv.y * (1 - uv.x) * (1 - uv.y);
		return clamp(16.0 * vignette, 0, 1);
	}


	float4 Frag(VaryingsDefault i): SV_Target
	{

		float overlay = SAMPLE_TEXTURE2D(_Overlay, sampler_Overlay, i.texcoordStereo).a;

		float vignette = Vignette(i.texcoordStereo);
		overlay = (overlay * _Params.y) ;
		vignette = (vignette / overlay);
		vignette = 1-saturate(vignette);

		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);

		float alpha = vignette * _Color.a * _Params.x;

		return float4(lerp(screenColor.rgb, _Color.rgb, alpha), screenColor.a);
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