Shader "Hidden/SC Post Effects/Lensflares"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"
	#include "../../../Shaders/Sampling.hlsl"
	#include "../../../Shaders/SCPE.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_BloomTex, sampler_BloomTex);
	TEXTURE2D_SAMPLER2D(_FlaresTex, sampler_FlaresTex);
	TEXTURE2D_SAMPLER2D(_ColorTex, sampler_ColorTex);
	TEXTURE2D_SAMPLER2D(_MaskTex, sampler_MaskTex);

	float4 _MainTex_TexelSize;
	float4 _FlaresTex_TexelSize;

	float _SampleDistance;
	float _Threshold;
	float _Distance;
	float _Falloff;
	float4 _Offsets;
	float _Intensity;
	float _Ghosts;
	float _HaloSize;
	float _HaloWidth;
	float _ChromaticAbberation;

	float4 _GhostParams;
	//X = NumGhost
	//Y = GhostDinstance
	//Z = GhostFalloff
	float4 _HaloParams;

	float4 FragLuminanceDiff(VaryingsDefault i) : SV_Target
	{
		return LuminanceThreshold(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord), _Threshold);
	}

	float4 FragGhosting(VaryingsDefault i) : SV_Target
	{
		//Flip bloom buffer
		float2 texcoord = -i.texcoord + 1.0;
		float2 centerVec = float2(0.5, 0.5);

		//Radial mask
		float2 center = texcoord * 2 - 1;
		float falloff = 1-dot(center, center) * _Falloff;
		falloff = saturate(falloff);

		//return float4(falloff, falloff, falloff, 1);

		//Ghosting
		float2 ghostVec = (centerVec - texcoord) * _Distance;

		float3 result = float3(0,0,0);
		for (int i = 0; i < _Ghosts; ++i)
		{
			float2 offset = frac(texcoord + ghostVec * float(i));

			result += SAMPLE_TEXTURE2D(_BloomTex, sampler_BloomTex, offset).rgb * falloff;
		}

		//Add halo
		float2 haloVec = normalize(centerVec - texcoord) * _HaloSize;
		float haloFalloff = length(float2(centerVec - frac(texcoord + haloVec))) * _HaloWidth;
		haloFalloff = pow(1.0 - haloFalloff, 5);
		//return float4(haloFalloff, haloFalloff, haloFalloff, 1);

		float halo = SAMPLE_TEXTURE2D(_BloomTex, sampler_BloomTex, texcoord + haloVec).r * (haloFalloff);
		result.rgb += halo * 1;

		//Add color ramp
		float4 colorRamp = SAMPLE_TEXTURE2D(_ColorTex, sampler_ColorTex, length(centerVec - texcoord) / 0.70).rgba;

		//Use color ramp alpha channel to blend
		result = lerp(result, result*colorRamp.rgb, colorRamp.a);

		result *= (_Intensity * 2);

		return float4(result, 1);
	}

		struct v2fGaussian {
		float4 vertex : SV_POSITION;
		float2 texcoord : TEXCOORD0;

		float4 uv01 : TEXCOORD2;
		float4 uv23 : TEXCOORD3;
		float4 uv45 : TEXCOORD4;
	};

	v2fGaussian VertGaussian(AttributesDefault v) {
		v2fGaussian o;
		o.vertex = float4(v.vertex.xy, 0, 1);

		o.texcoord = TransformTriangleVertexToUV(o.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
		o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
		//UNITY_SINGLE_PASS_STEREO
		o.texcoord = TransformStereoScreenSpaceTex(o.texcoord, 1.0);

		o.uv01 = o.texcoord.xyxy + _Offsets.xyxy * float4(1, 1, -1, -1);
		o.uv23 = o.texcoord.xyxy + _Offsets.xyxy * float4(1, 1, -1, -1) * 2.0;
		o.uv45 = o.texcoord.xyxy + _Offsets.xyxy * float4(1, 1, -1, -1) * 6.0;

		return o;
	}

	float4 FragGaussian(v2fGaussian i) : SV_Target
	{
		half4 color = float4(0, 0, 0, 0);

		color += 0.40 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
		color += 0.15 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv01.xy);
		color += 0.15 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv01.zw);
		color += 0.10 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv23.xy);
		color += 0.10 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv23.zw);
		color += 0.05 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv45.xy);
		color += 0.05 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv45.zw);

		return color;
	}

	float4 FragBlend(VaryingsDefault i) : SV_Target
	{
		float4 original = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
		float3 flares = SAMPLE_TEXTURE2D(_FlaresTex, sampler_FlaresTex, i.texcoord).rgb;
		float mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.texcoord).r;

		//CA
		if(_ChromaticAbberation > 0)
		{
			float2 direction = normalize((float2(0.5, 0.5) - i.texcoord));
			float3 distortion = float3(-_FlaresTex_TexelSize.x * _ChromaticAbberation, 0, _FlaresTex_TexelSize.x * _ChromaticAbberation);

			float red = SAMPLE_TEXTURE2D(_FlaresTex, sampler_FlaresTex, i.texcoord + direction * distortion.r).r;
			float green = SAMPLE_TEXTURE2D(_FlaresTex, sampler_FlaresTex, i.texcoord + direction * distortion.g).g;
			float blue = SAMPLE_TEXTURE2D(_FlaresTex, sampler_FlaresTex, i.texcoord + direction * distortion.b).b;

			flares = float3(red, green, blue);
		}

		flares *= mask;
		return float4(original.rgb + flares, original.a);
	}

	float4 FragDebug(VaryingsDefault i) : SV_Target
	{
		float4 original = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
		float3 flares = SAMPLE_TEXTURE2D(_FlaresTex, sampler_FlaresTex, i.texcoord).rgb;
		float mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.texcoord).r;

		//CA
		if (_ChromaticAbberation > 0)
		{
			float2 direction = normalize((float2(0.5, 0.5) - i.texcoord));
			float3 distortion = float3(-_FlaresTex_TexelSize.x * _ChromaticAbberation, 0, _FlaresTex_TexelSize.x * _ChromaticAbberation);

			float red = SAMPLE_TEXTURE2D(_FlaresTex, sampler_FlaresTex, i.texcoord + direction * distortion.r).r;
			float green = SAMPLE_TEXTURE2D(_FlaresTex, sampler_FlaresTex, i.texcoord + direction * distortion.g).g;
			float blue = SAMPLE_TEXTURE2D(_FlaresTex, sampler_FlaresTex, i.texcoord + direction * distortion.b).b;

			flares = float3(red, green, blue);
		}

		flares *= mask;

		return float4(flares.rgb, original.a);
	}

		ENDHLSL

		SubShader
	{
		Cull Off ZWrite Off ZTest Always

			Pass //0
		{
			HLSLPROGRAM

#pragma vertex VertDefault
#pragma fragment FragLuminanceDiff

			ENDHLSL
		}
			Pass //1
		{
			HLSLPROGRAM

#pragma vertex VertDefault
#pragma fragment FragGhosting

			ENDHLSL
		}
			Pass //2
		{
			HLSLPROGRAM

#pragma vertex VertGaussian
#pragma fragment FragGaussian

			ENDHLSL
		}
			Pass //3
		{
			HLSLPROGRAM

#pragma vertex VertDefault
#pragma fragment FragBlend

			ENDHLSL
		}
			Pass //4
		{
			HLSLPROGRAM

#pragma vertex VertDefault
#pragma fragment FragDebug

			ENDHLSL
		}
	}
}