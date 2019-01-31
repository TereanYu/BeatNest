Shader "Hidden/SC Post Effects/Sun Shafts"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"
	#include "../../../Shaders/SCPE.hlsl"
	#include "../../../Shaders/Blending.hlsl"

	TEXTURE2D_SAMPLER2D(_SunshaftBuffer, sampler_SunshaftBuffer);
	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);

	half _BlendMode;
	float4 _SunThreshold;
	float4 _SunColor;
	float4 _SunPosition;
	float _BlurRadius;

	struct v2f {
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
		float2 texcoordStereo : TEXCOORD1;
		float2 blurDir : TEXCOORD2;
	};

	v2f VertRadialBlur(AttributesDefault v)
	{
		v2f o;
		o.vertex = float4(v.vertex.xy, 0, 1);

		o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
		o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif

		//UNITY_SINGLE_PASS_STEREO
		o.texcoord = TransformStereoScreenSpaceTex(o.texcoord, 1.0);
		o.texcoordStereo = o.texcoord;

		o.blurDir = (_SunPosition.xy - o.texcoord.xy) * _BlurRadius;

		return o;
	}

	float4 FragSky(VaryingsDefault i) : SV_Target
	{
		float depthSample = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoordStereo));
		float4 skyColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);

		half2 vec = _SunPosition.xy - i.texcoord.xy;
		half dist = saturate(_SunPosition.w - length(vec.xy));

		float4 outColor = 0;
		//reject near depth pixels
		if (depthSample > 0.99) {
			outColor = dot(max(skyColor.rgb - _SunThreshold.rgb, half3(0, 0, 0)), half3(1, 1, 1)) * dist;
		}

		return outColor * _SunPosition.z;
	}

	float4 FragRadialBlur(v2f i) : SV_Target
	{
		half4 c = half4(0,0,0, 0);
		for (int s = 0; s < 6; s++)
		{
			half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo).rgba;
			c += color;
			i.texcoordStereo.xy += i.blurDir;
		}
		return c / 6;
	}

	float4 FragBlend(VaryingsDefault i) : SV_Target
	{
		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);
		float3 sunshafts = SAMPLE_TEXTURE2D(_SunshaftBuffer, sampler_SunshaftBuffer, i.texcoordStereo).rgb;
		sunshafts.rgb *= _SunColor.rgb;

		float3 blendedColor = 0;

		if (_BlendMode == 0) blendedColor = BlendAdditive(screenColor.rgb, sunshafts.rgb); //Additive blend
		if (_BlendMode == 1) blendedColor = BlendScreen(sunshafts.rgb, screenColor.rgb); //Screen blend

		return float4(blendedColor.rgb, screenColor.a);
	}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragSky

			ENDHLSL
		}
		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertRadialBlur
			#pragma fragment FragRadialBlur

			ENDHLSL
		}
		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragBlend

			ENDHLSL
		}
	}
}