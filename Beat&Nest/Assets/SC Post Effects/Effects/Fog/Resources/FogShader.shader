Shader "Hidden/SC Post Effects/Fog"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"
	#include "../../../Shaders/SCPE.hlsl"
	#include "../../../Shaders/Blending.hlsl"
	#include "../../../Shaders/Sampling.hlsl"
	#include "../../../Shaders/Colors.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
	TEXTURE2D_SAMPLER2D(_NoiseTex, sampler_NoiseTex);
	TEXTURE2D_SAMPLER2D(_ColorGradient, sampler_ColorGradient);
	TEXTURE2D_SAMPLER2D(_SkyboxTex, sampler_SkyboxTex);

	#pragma fragmentoption ARB_precision_hint_nicest

	uniform float4 _ViewDir;
	uniform half _FarClippingPlane;
	uniform float4 _HeightParams;
	uniform float4 _DistanceParams;
	uniform int4 _SceneFogMode;
	uniform float4 _SceneFogParams;
	uniform float4 _DensityParams;
	uniform float4 _NoiseParams;
	uniform float4 _FogColor;
	uniform float4x4 clipToWorld;
	float4 _MainTex_TexelSize;

	//Light scattering
	TEXTURE2D_SAMPLER2D(_BloomTex, sampler_BloomTex);
	TEXTURE2D_SAMPLER2D(_AutoExposureTex, sampler_AutoExposureTex);
	float  _SampleScale;
	float4 _Threshold; // x: threshold value (linear), y: threshold - knee, z: knee * 2, w: 0.25 / knee
	float4 _ScatteringParams; // x: Sample scale y: Intensity z: 0 w: Itterations

	struct v2f {
		float4 vertex : SV_POSITION;
		float2 texcoord : TEXCOORD0;
		float2 texcoordStereo : TEXCOORD1;
		float3 worldDirection : TEXCOORD2;
		float time : TEXCOORD3;
	};

	v2f Vert(AttributesDefault v) {
		v2f o;
		o.vertex = float4(v.vertex.xy, 0.0, 1.0);
		o.texcoord.xy = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
		o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
		float4 clip = float4(o.texcoord.xy * 2 - 1, 0.0, 1.0);
		o.worldDirection = (mul(clipToWorld, clip.rgba) - _WorldSpaceCameraPos).xyz;
		o.time = _Time.y;

		//UNITY_SINGLE_PASS_STEREO
		o.texcoordStereo = TransformStereoScreenSpaceTex(o.texcoord, 1.0);

		return o;
	}

	half ComputeFogFactor(float coord)
	{
		float fogFac = 0.0;
		if (_SceneFogMode.x == 1) // linear
		{
			// factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
			fogFac = coord * _SceneFogParams.z + _SceneFogParams.w;
		}
		if (_SceneFogMode.x == 2) // exp
		{
			// factor = exp(-density*z)
			fogFac = _SceneFogParams.y * coord; fogFac = exp2(-fogFac);
		}
		if (_SceneFogMode.x == 3) // exp2
		{
			// factor = exp(-(density*z)^2)
			fogFac = _SceneFogParams.x * coord; fogFac = exp2(-fogFac * fogFac);
		}
		return saturate(fogFac);
	}

	float ComputeDistance(float3 wpos, float depth)
	{
		float3 wsDir = _WorldSpaceCameraPos.xyz - wpos;
		float dist;
		//Radial distance
		if (_SceneFogMode.y == 1)
			dist = length(wsDir);
		else
			dist = depth * _ProjectionParams.z;
		//Start distance
		dist -= _ProjectionParams.y;
		//Density
		dist *= _DensityParams.x;
		return dist;
	}

	float ComputeHeight(float3 wpos)
	{
		float3 wsDir = _WorldSpaceCameraPos.xyz - wpos;
		float FH = _HeightParams.x;
		float3 C = _WorldSpaceCameraPos;
		float3 V = wsDir;
		float3 P = wpos;
		float3 aV = _HeightParams.w * V;
		float FdotC = _HeightParams.y;
		float k = _HeightParams.z;
		float FdotP = P.y - FH;
		float FdotV = wsDir.y;
		float c1 = k * (FdotP + FdotC);
		float c2 = (1 - 2 * k) * FdotP;
		float g = min(c2, 0.0);
		g = -length(aV) * (c1 - g * g / abs(FdotV + 1.0e-5f));
		return g;
	}

	float4 ComputeFog(v2f i) {

		half4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);

		float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoordStereo);
		float depth = Linear01Depth(rawDepth);

		float skyMask = 1;
		if (depth > 0.99) skyMask = 0;

		float3 worldPos = i.worldDirection * LinearEyeDepth(rawDepth) + _WorldSpaceCameraPos;

		//Fog start distance
		float g = _DistanceParams.x;

		//Distance fog
		float distanceFog = 0;
		if (_DistanceParams.z == 1) {
			distanceFog = ComputeDistance(worldPos, depth);
			g += distanceFog;
		}

		//Height fog
		float heightFog = 0;
		if (_DistanceParams.w == 1) {
			float noise = 1;
			if (_SceneFogMode.w == 1)
			{
				noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, worldPos.xz * _NoiseParams.x + (i.time * _NoiseParams.y * float2(0, 1))).r;
				noise = lerp(1, noise, _DensityParams.y * skyMask);
			}
			heightFog = ComputeHeight(worldPos);
			g += heightFog * noise;
		}

		//Fog density
		half fogFac = ComputeFogFactor(max(0.0, g));

		//Exclude skybox
		if (depth == _DistanceParams.y)
			fogFac = 1.0;

		//Color
		float4 fogColor = _FogColor.rgba;
		if (_SceneFogMode.z == 1)
		{
			fogColor = SAMPLE_TEXTURE2D(_ColorGradient, sampler_ColorGradient, float2(LinearEyeDepth(rawDepth) / _FarClippingPlane, 0));
		}
		if (_SceneFogMode.z == 2) {
			fogColor = SAMPLE_TEXTURE2D_LOD(_SkyboxTex, sampler_SkyboxTex, i.texcoord, 6);
		}

		fogColor.a = fogFac;

		return fogColor;
	}

	half4 Prefilter(half4 color, float2 uv)
	{
		half autoExposure = SAMPLE_TEXTURE2D(_AutoExposureTex, sampler_AutoExposureTex, uv).r;
		color *= autoExposure;
		//color = min(_Params.x, color); // clamp to max
		color = QuadraticThreshold(color, _Threshold.x, _Threshold.yzw);
		return color;
	}

	half4 FragPrefilter(VaryingsDefault i) : SV_Target
	{
		half4 color = DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, _MainTex_TexelSize.xy);
		return Prefilter(SafeHDR(color), i.texcoord);
	}

	half4 FragDownsample(VaryingsDefault i) : SV_Target
	{
		half4 color = DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, _MainTex_TexelSize.xy);
		return color;
	}

	half4 Combine(half4 bloom, float2 uv)
	{
		half4 color = SAMPLE_TEXTURE2D(_BloomTex, sampler_BloomTex, uv);
		return bloom + color;
	}

	half4 FragUpsample(VaryingsDefault i) : SV_Target
	{
		half4 bloom = UpsampleBox(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, _MainTex_TexelSize.xy, _SampleScale);
		return Combine(bloom, i.texcoordStereo);
	}

	float4 FragBlend(v2f i) : SV_Target
	{
		half4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);

		//Alpha is density, do not modify
		float4 fogColor = ComputeFog(i);

		//screenColor.rgb = lerp(bloom.rgb, screenColor.rgb, fogColor.a);

		//Linear blend
		float3 blendedColor = lerp(fogColor.rgb, screenColor.rgb, fogColor.a);

		//Keep alpha channel for FXAA
		return float4(blendedColor.rgb, screenColor.a);
	}

	float4 FragBlendScattering(v2f i) : SV_Target
	{
		half4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);
		half4 bloom = SAMPLE_TEXTURE2D(_BloomTex, sampler_BloomTex, i.texcoordStereo) * _ScatteringParams.y;

		//return bloom;

		//Alpha is density, do not modify
		float4 fogColor = ComputeFog(i);

		fogColor.rgb = fogColor.rgb + bloom.rgb;

		screenColor.rgb = lerp(bloom.rgb, screenColor.rgb, fogColor.a);

		//Linear blend
		float3 blendedColor = lerp(fogColor.rgb, screenColor.rgb, fogColor.a);

		//Keep alpha channel for FXAA
		return float4(blendedColor.rgb, screenColor.a);
	}


	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass //0
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragPrefilter

			ENDHLSL
		}

		Pass //1
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragDownsample

			ENDHLSL
		}
		Pass //2
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragUpsample

			ENDHLSL
		}
		Pass //3
		{
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment FragBlend

			ENDHLSL
		}
		Pass //4
		{
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment FragBlendScattering

			ENDHLSL
		}
	}
}