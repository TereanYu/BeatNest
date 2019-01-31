Shader "Hidden/SC Post Effects/Ambient Occlusion 2D"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"
	#include "../../../Shaders/Sampling.hlsl"
	#include "../../../Shaders/SCPE.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_AO, sampler_AO);
	float4 _MainTex_TexelSize;
	float _SampleDistance;
	float _Threshold;
	float4 _BlurOffsets;
	float _Blur;
	float _Intensity;

	struct v2flum {
		float4 vertex : POSITION;
		float2 texcoord[3] : TEXCOORD0;
		float2 texcoordStereo : TEXCOORD3;
	};

	v2flum Vert(AttributesDefault v)
	{
		v2flum o;
		o.vertex = float4(v.vertex.xy, 0, 1);
		float2 uv = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
		uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
		//UNITY_SINGLE_PASS_STEREO
		o.texcoordStereo = TransformStereoScreenSpaceTex(uv, 1.0);

		o.texcoord[0] = uv;
		o.texcoord[1] = uv + float2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * _SampleDistance;
		o.texcoord[2] = uv + float2(+_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * _SampleDistance;

		return o;
	}

	float4 FragLuminanceDiff(v2flum i) : SV_Target
	{
		float4 original = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord[0]);

		// a very simple cross gradient filter

		half3 p1 = original.rgb;
		half3 p2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord[1]).rgb;
		half3 p3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord[2]).rgb;

		half3 diff = p1 * 2 - p2 - p3;
		half edge = dot(diff, diff);
		edge = step(edge, _Threshold);

		//Edges only
		original.rgb = lerp(1, edge, _Intensity);

		//return original;
		return original;
	}


	struct v2fGaussian {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;

		float4 uv01 : TEXCOORD1;
		float4 uv23 : TEXCOORD2;
		float4 uv45 : TEXCOORD3;
	};

	v2fGaussian VertGaussian(AttributesDefault v) {
		v2fGaussian o;
		o.pos = float4(v.vertex.xy, 0, 1);

		o.uv.xy = TransformTriangleVertexToUV(o.pos.xy);

#if UNITY_UV_STARTS_AT_TOP
		o.uv = o.uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
		//UNITY_SINGLE_PASS_STEREO
		o.uv = TransformStereoScreenSpaceTex(o.uv, 1.0);

		o.uv01 = o.uv.xyxy + _BlurOffsets.xyxy * float4(1, 1, -1, -1);
		o.uv23 = o.uv.xyxy + _BlurOffsets.xyxy * float4(1, 1, -1, -1) * 2.0;
		o.uv45 = o.uv.xyxy + _BlurOffsets.xyxy * float4(1, 1, -1, -1) * 6.0;

		return o;
	}

	float4 FragBlurGaussian(v2fGaussian i) : SV_Target
	{
		half4 color = float4(0, 0, 0, 0);

		color += 0.40 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
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
		float4 original = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);
		float ao = SAMPLE_TEXTURE2D(_AO, sampler_AO, i.texcoordStereo).r;

		return float4(original.rgb * ao, original.a);
	}

	float4 FragDebug(VaryingsDefault i) : SV_Target
	{
		 return SAMPLE_TEXTURE2D(_AO, sampler_AO, i.texcoordStereo);
	}

	ENDHLSL

SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass //0
		{
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment FragLuminanceDiff

			ENDHLSL
		}
		Pass //1
		{
			HLSLPROGRAM

			#pragma vertex VertGaussian
			#pragma fragment FragBlurGaussian

			ENDHLSL
		}
		Pass //2
		{
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment FragBlend

			ENDHLSL
		}
		Pass //3
		{
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment FragDebug

			ENDHLSL
		}
	}
}