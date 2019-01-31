Shader "Hidden/SC Post Effects/Sketch"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_Strokes, sampler_Strokes);
	TEXTURE2D_SAMPLER2D(_StrokesDark, sampler_StrokesDark);
	TEXTURE2D_SAMPLER2D(_StrokesLight, sampler_StrokesLight);
	TEXTURE2D_SAMPLER2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture);
	TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);

	uniform float4 _Params;
	//X: Projection mode
	//Y: Blending mode
	//Z: Intensity
	//W: Tiling
	uniform float4 _Brightness;

	uniform float _Intensity;
	uniform float _Tiling;
	uniform float _LuminanceThreshold;

	uniform float4x4 clipToWorld;

	struct v2f {
		float4 vertex : SV_POSITION;
		float2 texcoord : TEXCOORD0;
		float2 texcoordStereo : TEXCOORD1;
		float3 worldDirection : TEXCOORD2;
	};

	v2f Vert(AttributesDefault v) {
		v2f o;
		o.vertex = float4(v.vertex.xy, 0.0, 1.0);
		o.texcoord.xy = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
		o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
		float4 clip = float4(o.texcoord.xy * 2 - 1, 0.0, 1.0);
		o.worldDirection.rgb = (mul((float4x4)clipToWorld, clip.rgba) - _WorldSpaceCameraPos.rgb).xyz;

		//UNITY_SINGLE_PASS_STEREO
		o.texcoordStereo = TransformStereoScreenSpaceTex(o.texcoord, 1.0);

		return o;
	}

	float Hatching(float2 uv, float NdotL) {
		half hatch = saturate(1 - NdotL);

		half3 tex = SAMPLE_TEXTURE2D(_Strokes, sampler_Strokes, uv).rgb;

		float dark = smoothstep(0, hatch, tex.r) + _Brightness.x;
		float light = smoothstep(0, hatch, tex.g) * _Brightness.y;

		hatch = lerp(dark, light, NdotL);

		return saturate(hatch);
	}

	float3 Blend(float3 color, float3 hatch, float lum) {
		float3 col = color.rgb;

		//Effect-only
		if (_Params.y == 0)
			col = lerp(color.rgb, hatch.rgb, _Params.z);
		//Multiply
		if (_Params.y == 1)
			col = lerp(color.rgb, color.rgb * hatch.rgb, _Params.z);

		//Add
		if (_Params.y == 2)
		{
			col = lerp(color.rgb, color.rgb + hatch.rgb, _Params.z);
		}

		return saturate(col);
	}

	float4 FragWorldSpace(v2f i) : SV_Target
	{
		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);

		float depth = (SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoordStereo));

		//float3 normals = DecodeViewNormalStereo(SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, i.uv));
		//return float4(normals.y, normals.y, normals.y, 1);

		float3 worldPos = i.worldDirection * LinearEyeDepth(depth) + _WorldSpaceCameraPos;

		float3 worldUV = worldPos.xyz * 0.01 * _Params.w;
		float2 uvX = worldUV.yz;
		float2 uvY = worldUV.xz;
		float2 uvZ = worldUV.xy;

		//Use luminance to create a psuedo diffuse light weight
		float luminance = dot(screenColor.rgb, float3(0.2326, 0.7152, 0.0722));

		//return luminance;

		float hatchX = Hatching(uvX, luminance);
		float hatchY = Hatching(uvY, luminance);
		float hatchZ = Hatching(uvZ, luminance);

		//float3 hatch = (hatchX * hatchY * hatchZ);
		float3 hatch = (hatchX + hatchY + hatchZ) * 0.33;
		hatch = saturate(hatch);

		if (depth > 1000) hatch = 1.0;

		float3 col = Blend(screenColor.rgb, hatch.rgb, luminance);

		return float4(col.rgb, screenColor.a);
	}

		float4 FragScreenSpace(v2f i) : SV_Target
	{
		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);

		half luminance = dot(screenColor.rgb, half3(0.2326, 0.7152, 0.0722));

		float3 hatch = Hatching(i.texcoord * _Params.w, luminance);


		float3 col = Blend(screenColor.rgb, hatch.rgb, luminance);

		return float4(col.rgb, screenColor.a);
	}

		ENDHLSL

		SubShader
	{
		Cull Off ZWrite Off ZTest Always

			Pass //0
		{
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment FragWorldSpace

			ENDHLSL
		}

			Pass //1
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragScreenSpace

			ENDHLSL
		}
	}
}