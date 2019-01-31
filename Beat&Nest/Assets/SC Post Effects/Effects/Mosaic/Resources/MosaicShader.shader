Shader "Hidden/SC Post Effects/Mosaic"
{
		HLSLINCLUDE

		#include "../../../Shaders/StdLib.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		float4 _Params;

		float2 TrianglesUV(float2 uv, float size) {

			float2 coord = floor(uv*size) / size;
			uv -= coord;
			uv *= size;

			//Intersect
			return coord + float2(
				//X
				step(1 - uv.y, uv.x) / (size),
				//Y                                            
				step(uv.x, uv.y) / (size)
			);
		}

		inline float mod(float2 a, float b)
		{
			return (a.xy - (b * floor(a.xy / b))).x;
		}

		float2 triChecker(float2 uv, float size)
		{
			float2 m = mod(uv, size);
			float2 base = uv - m;
			uv = m / size;

			base.x *=  step(uv.x, uv.y);

			return base;
		}


		float2 SquareUV(float2 uv, float size) {
			float dx = 10*(1.0 /size);
			float dy = 10*(1.0 / size);
			float2 coord = float2(dx*floor(uv.x / dx),
				dy*floor(uv.y / dy));

			return coord;
		}

		//Translated from http://coding-experiments.blogspot.nl/2010/06/pixelation.html
		//Modified for Unity
		float2 HexUV(float2 hexIndex) {
			int i = hexIndex.x;
			int j = hexIndex.y;
			float2 r;
			r.x = i * _Params.x;
			r.y = j * _Params.y + (i % 2.0) * _Params.y / 2.0;
			return r;
		}

		float2 HexIndex(float2 uv, float size) {

			float2 r;

			int it = int(floor(uv.x / size));
			float yts = uv.y - float(it % 2.0) * _Params.y / 2.0;
			int jt = int(floor((1.0 / _Params.y) * yts));
			float xt = uv.x - it * size;
			float yt = yts - jt * _Params.y;
			int deltaj = (yt > _Params.y / 2.0) ? 1 : 0;
			float fcond = size * (2.0 / 3.0) * abs(0.5 - yt / _Params.y);

			if (xt > fcond) {
				r.x = it;
				r.y = jt;
			}
			else {
				r.x = it - 1;
				r.y = jt - (r.x % 2) + deltaj;
			}

			return r;
		}

		float4 FragTriangles(VaryingsDefault i) : SV_Target
		{
			float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, TrianglesUV(i.texcoordStereo, _Params.x));

			return screenColor;
		}

		float4 FragHex(VaryingsDefault i) : SV_Target
		{
			float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, HexUV(HexIndex(i.texcoordStereo, _Params.x)));

			return screenColor;
		}

		float4 FragSquare(VaryingsDefault i) : SV_Target
		{
			float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, SquareUV(i.texcoordStereo, _Params.x));

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
			#pragma fragment FragTriangles

			ENDHLSL
		}

			Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragHex

			ENDHLSL
		}
	}
}