Shader "Hidden/SC Post Effects/Edge Detection" {

	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"

	//Screen color
	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	//Unused but required by UnityStereoScreenSpaceUVAdjust
	float4 _MainTex_ST;
	uniform float4 _MainTex_TexelSize;

	//Camera depth textures
	TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
	TEXTURE2D_SAMPLER2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture);

	//Parameters
	uniform half4 _Sensitivity;
	uniform half _BackgroundFade;
	uniform float _EdgeSize;
	uniform float4 _EdgeColor;
	uniform float _Exponent;
	uniform float _Threshold;
	uniform float4 _DistanceParams;

	uniform float4 _SobelParams;

	//Structs
	struct v2f {
		float4 vertex : POSITION;
		float2 texcoord[5] : TEXCOORD0;
	};

	struct v2fRobert {
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
		float4 screenCoord[2] : TEXCOORD1;
	};

	struct v2fSobel {
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
		float4 corners[2] : TEXCOORD1;
	};

	struct v2flum {
		float4 vertex : POSITION;
		float2 texcoord[3] : TEXCOORD0;
	};

	inline float DecodeFloatRG(float2 enc)
	{
		float2 kDecodeDot = float2(1.0, 1 / 255.0);
		return dot(enc, kDecodeDot);
	}

	inline float DistanceFade(float depth) {
		float distanceFade = Linear01Depth(depth) * _DistanceParams.x;
		distanceFade = saturate(distanceFade);

		if (_DistanceParams.y == 1) distanceFade = 1 - distanceFade;

		return distanceFade;
	}

	inline half IsSame(half2 centerNormal, float centerDepth, half4 theSample)
	{
		// difference in normals
		half2 diff = abs(centerNormal - theSample.xy) * _Sensitivity.y;
		half isSameNormal = (diff.x + diff.y) * _Sensitivity.y < 0.1;
		// difference in depth
		float sampleDepth = DecodeFloatRG(theSample.zw);
		float zdiff = abs(centerDepth - sampleDepth);
		// scale the required threshold by the distance
		half isSameDepth = zdiff * _Sensitivity.x < 0.09 * centerDepth;

		// return:
		// 1 - if normals and depth are similar enough
		// 0 - otherwise

		return isSameNormal * isSameDepth;
	}

	//TRIANGLE DEPTH NORMALS METHOD

	v2f vertDNormals(AttributesDefault v)
	{
		v2f o;
		o.vertex = float4(v.vertex.xy, 0, 1);

		float2 uv = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
		uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif

		//UNITY_SINGLE_PASS_STEREO
		uv = TransformStereoScreenSpaceTex(uv, 1.0);

		o.texcoord[0] = uv;

		// offsets for two additional samples
		o.texcoord[1] = uv + float2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * _EdgeSize;
		o.texcoord[2] = uv + float2(+_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * _EdgeSize;
		o.texcoord[3] = uv;
		o.texcoord[4] = uv;

		return o;
	}

	half4 fragDNormals(v2f i) : SV_Target
	{
		half4 original = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord[0]);

		half4 center = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, i.texcoord[0].xy);
		half4 sample1 = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, i.texcoord[1].xy);
		half4 sample2 = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, i.texcoord[2].xy);

		// encoded normal
		half2 centerNormal = center.xy;
		// decoded depth
		float centerDepth = DecodeFloatRG(center.zw);

		half edge = 1;
		edge *= IsSame(centerNormal, centerDepth, sample1);
		edge *= IsSame(centerNormal, centerDepth, sample2);
		edge = 1 - edge;

		//Edges only
		original = lerp(original, float4(1, 1, 1, 1), _BackgroundFade);

		//Opacity
		float3 edgeColor = lerp(original.rgb, _EdgeColor.rgb, _EdgeColor.a * DistanceFade(centerDepth));
		edgeColor = saturate(edgeColor);

		return float4(lerp(original.rgb, edgeColor.rgb, edge).rgb, original.a);

	}

		//ROBERTS CROSS DEPTH NORMALs METHOD

		v2fRobert vertRobert(v2fRobert v)
	{
		v2fRobert o;
		o.vertex = float4(v.vertex.xy, 0, 1);

		float2 uv = TransformTriangleVertexToUV(v.vertex.xy);


#if UNITY_UV_STARTS_AT_TOP
		uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
		//UNITY_SINGLE_PASS_STEREO
		uv = TransformStereoScreenSpaceTex(uv, 1.0);

		o.texcoord = uv;

		// calc coord for the X pattern
		// maybe nicer TODO for the future: 'rotated triangles'

		o.screenCoord[0].xy = uv + _MainTex_TexelSize.xy * half2(1, 1) * _EdgeSize;
		o.screenCoord[0].zw = uv + _MainTex_TexelSize.xy * half2(-1, -1) * _EdgeSize;
		o.screenCoord[1].xy = uv + _MainTex_TexelSize.xy * half2(-1, 1) * _EdgeSize;
		o.screenCoord[1].zw = uv + _MainTex_TexelSize.xy * half2(1, -1) * _EdgeSize;

		return o;
	}

	half4 fragRobert(v2fRobert i) : SV_Target
	{
		half4 original = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);

		half4 sample1 = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, i.screenCoord[0].xy);
		half4 sample2 = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, i.screenCoord[0].zw);
		half4 sample3 = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, i.screenCoord[1].xy);
		half4 sample4 = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, i.screenCoord[1].zw);

		float centerDepth = DecodeFloatRG(sample1.zw);
		float depth = Linear01Depth(centerDepth) * _DistanceParams.x;

		half edge = 1.0;

		edge *= IsSame(sample1.xy, DecodeFloatRG(sample1.zw), sample2);
		edge *= IsSame(sample3.xy, DecodeFloatRG(sample3.zw), sample4);

		edge = 1 - edge;

		//Edges only
		original = lerp(original, float4(1, 1, 1, 1), _BackgroundFade);

		//Opacity
		float3 edgeColor = lerp(original.rgb, _EdgeColor.rgb, _EdgeColor.a * DistanceFade(centerDepth));

		//return original;
		return float4(lerp(original.rgb, edgeColor.rgb, edge).rgb, original.a);
	}

	//SOBEL DEPTH METHOD

	v2fSobel vertSobel(v2fSobel v)
	{
		v2fSobel o;
		o.vertex = float4(v.vertex.xy, 0, 1);

		float2 uv = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
		uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
		//UNITY_SINGLE_PASS_STEREO
		uv = TransformStereoScreenSpaceTex(uv, 1.0);

		o.texcoord = uv;

		float2 uvDist = _EdgeSize * _MainTex_TexelSize.xy;

		//Top-right
		o.corners[0].xy = uv + uvDist;
		//Top-left
		o.corners[0].zw = uv + uvDist * half2(-1, 1);
		//Bottom-right
		o.corners[1].xy = uv - uvDist * half2(-1, 1);
		//Bottom-left
		o.corners[1].zw = uv - uvDist;

		return o;
	}

	float4 fragSobel(v2fSobel i) : SV_Target
	{
		// inspired by borderlands implementation of popular "sobel filter"
		half4 original = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);

		float centerDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord));
		float4 depthsDiag;
		float4 depthsAxis;

		depthsDiag.x = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.corners[0].xy)); // TR
		depthsDiag.y = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.corners[0].zw)); // TL
		depthsDiag.z = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.corners[1].xy)); // BR
		depthsDiag.w = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.corners[1].zw)); // BL

		float2 uvDist = _EdgeSize * _MainTex_TexelSize.xy;

		depthsAxis.x = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord + uvDist * half2(0, 1))); // T
		depthsAxis.y = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord - uvDist * half2(1, 0))); // L
		depthsAxis.z = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord + uvDist * half2(1, 0))); // R
		depthsAxis.w = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord - uvDist * half2(0, 1))); // B	

		//Thin edges
		if (_SobelParams.x == 1) {
			depthsDiag = (depthsDiag > centerDepth.xxxx) ? depthsDiag : centerDepth.xxxx;
			depthsAxis = (depthsAxis > centerDepth.xxxx) ? depthsAxis : centerDepth.xxxx;
		}
		depthsDiag -= centerDepth;
		depthsAxis /= centerDepth;

		const float4 HorizDiagCoeff = float4(1,1,-1,-1);
		const float4 VertDiagCoeff = float4(-1,1,-1,1);
		const float4 HorizAxisCoeff = float4(1,0,0,-1);
		const float4 VertAxisCoeff = float4(0,1,-1,0);

		float4 SobelH = depthsDiag * HorizDiagCoeff + depthsAxis * HorizAxisCoeff;
		float4 SobelV = depthsDiag * VertDiagCoeff + depthsAxis * VertAxisCoeff;

		float SobelX = dot(SobelH, float4(1,1,1,1));
		float SobelY = dot(SobelV, float4(1,1,1,1));
		float Sobel = sqrt(SobelX * SobelX + SobelY * SobelY);

		Sobel = 1.0 - pow(saturate(Sobel), _Exponent);

		float edge = 1 - Sobel;

		//Orthographic camera: Still not correct, by value should be flipped
		if (unity_OrthoParams.w) edge = 1 - edge;

		//Edges only
		original = lerp(original, float4(1, 1, 1, 1), _BackgroundFade);

		//Opacity
		float3 edgeColor = lerp(original.rgb, _EdgeColor.rgb, _EdgeColor.a * DistanceFade(centerDepth));

		return float4(lerp(original.rgb, edgeColor.rgb, edge).rgb, original.a);
	}

		//TRIANGLE LUMINANCE VARIANCE METHOD

		v2flum vertLum(AttributesDefault v)
	{
		v2flum o;
		o.vertex = float4(v.vertex.xy, 0, 1);
		float2 uv = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
		uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
		//UNITY_SINGLE_PASS_STEREO
		uv = TransformStereoScreenSpaceTex(uv, 1.0);

		o.texcoord[0] = uv;
		o.texcoord[1] = uv + float2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * _EdgeSize;
		o.texcoord[2] = uv + float2(+_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * _EdgeSize;

		return o;
	}

	float4 fragLum(v2flum i) : SV_Target
	{
		float4 original = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord[0]);

		float centerDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord[0]));
		float depth = centerDepth * _DistanceParams.x;

		half3 p1 = original.rgb;
		half3 p2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord[1]).rgb;
		half3 p3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord[2]).rgb;

		half3 diff = p1 * 2 - p2 - p3;
		half edge = dot(diff, diff);
		edge = step(edge, _Threshold);

		edge = 1 - edge;

		//Edges only
		original = lerp(original, float4(1, 1, 1, 1), _BackgroundFade);



		//Opacity
		float3 edgeColor = lerp(original.rgb, _EdgeColor.rgb, _EdgeColor.a * DistanceFade(centerDepth));
		edgeColor = saturate(edgeColor);

		//return original;
		return float4(lerp(original.rgb, edgeColor.rgb, edge).rgb, original.a);
	}


		ENDHLSL

		//Pass determined by EdgeDetectionMode enum value
		Subshader {
		Pass{
			 ZTest Always Cull Off ZWrite Off

			 HLSLPROGRAM
			 #pragma vertex vertDNormals
			 #pragma fragment fragDNormals
			ENDHLSL
		}
			Pass{
				 ZTest Always Cull Off ZWrite Off

				 HLSLPROGRAM
				 #pragma vertex vertRobert
				 #pragma fragment fragRobert
				ENDHLSL
		}
			Pass{
				 ZTest Always Cull Off ZWrite Off

				 HLSLPROGRAM
				 #pragma vertex vertSobel
				 #pragma fragment fragSobel
				ENDHLSL
		}
			Pass{
				 ZTest Always Cull Off ZWrite Off

				 HLSLPROGRAM
				 #pragma vertex vertLum
				 #pragma fragment fragLum
				ENDHLSL
		}
	}

	Fallback off

} // shader
