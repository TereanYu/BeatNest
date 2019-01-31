Shader "Hidden/SC Post Effects/Color Split"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"
	#include "../../../Shaders/Sampling.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	uniform float4 _MainTex_TexelSize;

	float _Offset;

	//Shorthand for sampling MainTex
	inline float4 screenColor(float2 uv) {
		return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
	}

	//Structs
	struct v2f
	{
		float4 vertex: POSITION;
		float2 texcoord[3]: TEXCOORD0;
		//float2 texcoordStereo : TEXCOORD4;
	};

	struct v2fDouble
	{
		float4 vertex: POSITION;
		float2 texcoord[5]: TEXCOORD0;
		float2 texcoordStereo : TEXCOORD5;
	};

	v2f VertSingle(AttributesDefault v)
	{
		v2f o;
		o.vertex = float4(v.vertex.xy, 0.0, 1.0);

		float2 uv = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
		uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
		//UNITY_SINGLE_PASS_STEREO
		uv = TransformStereoScreenSpaceTex(uv, 1.0);

		o.texcoord[0] = uv;
		o.texcoord[1] = uv - float2(_Offset, 0);
		o.texcoord[2] = uv + float2(_Offset, 0);

		return o;
	}

	float4 FragSingle(v2f i) : SV_Target
	{
		float red = screenColor(i.texcoord[1]).r;
		float4 original = screenColor(i.texcoord[0]);
		float blue = screenColor(i.texcoord[2]).b;

		float4 splitColors = float4(red, original.g, blue, original.a);

		return splitColors;
	}

	float4 FragSingleBoxFiltered(v2f i): SV_Target
	{

		float red = DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord[1], _MainTex_TexelSize.xy * (_Offset * 200)).r;
		float4 original = DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord[0], _MainTex_TexelSize.xy * (_Offset * 200));
		float blue = DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord[2], _MainTex_TexelSize.xy * (_Offset * 200)).b;

		float4 splitColors = float4(red, original.g, blue, original.a);

		return splitColors;
	}

	v2fDouble VertDouble(AttributesDefault v)
	{
		v2fDouble o;
		o.vertex = float4(v.vertex.xy, 0, 1);

		float2 uv = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
		uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
		//UNITY_SINGLE_PASS_STEREO
		uv = TransformStereoScreenSpaceTex(uv, 1.0);

		o.texcoord[0] = uv;

		//X
		o.texcoord[1] = uv - float2(_Offset, 0);
		o.texcoord[2] = uv + float2(_Offset, 0);

		//Y
		o.texcoord[3] = uv - float2(0, _Offset);
		o.texcoord[4] = uv + float2(0, _Offset);

		o.texcoordStereo = TransformStereoScreenSpaceTex(o.texcoord[0], 1.0);

		return o;
	}

	float4 FragDouble(v2fDouble i) : SV_Target
	{

		float redX = screenColor(i.texcoord[1]).r;
		float redY = screenColor(i.texcoord[3]).r;

		float4 original = screenColor(i.texcoord[0]);

		float blueX = screenColor(i.texcoord[2]).b;
		float blueY = screenColor(i.texcoord[4]).b;


		float4 splitColorsX = float4(redX, original.g, blueX, original.a);
		float4 splitColorsY = float4(redY, original.g, blueY, original.a);

		float4 blendedColors = (splitColorsX + splitColorsY) /2;

		return blendedColors;
	}

	float4 FragDoubleBoxFiltered(v2fDouble i) : SV_Target
	{

		float4 redX = DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord[1], _MainTex_TexelSize.xy * (_Offset * 200));
		float4 redY = DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord[3], _MainTex_TexelSize.xy * (_Offset * 200));

		float4 original = DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord[0], _MainTex_TexelSize.xy * (_Offset * 200));

		float4 blueX = DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord[2], _MainTex_TexelSize.xy * (_Offset * 200));
		float4 blueY = DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord[4], _MainTex_TexelSize.xy * (_Offset * 200));


		float4 splitColorsX = float4(redX.r, original.g, blueX.b, original.a);
		float4 splitColorsY = float4(redY.r, original.g, blueY.b, original.a);

		float4 blendedColors = (splitColorsX + splitColorsY) /2;

		return blendedColors;
	}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		//Single
		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertSingle
			#pragma fragment FragSingle

			ENDHLSL
		}
			//Single Box Filtered
			Pass
		{
			HLSLPROGRAM

			#pragma vertex VertSingle
			#pragma fragment FragSingleBoxFiltered

			ENDHLSL
		}
			//Double
		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDouble
			#pragma fragment FragDouble

			ENDHLSL
		}
			//Double Box Filtered
			Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDouble
			#pragma fragment FragDoubleBoxFiltered

			ENDHLSL
		}
	}
}