Shader "Hidden/SC Post Effects/Cloud Shadows"
{
	HLSLINCLUDE

	#include "../../../Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
	TEXTURE2D_SAMPLER2D(_NoiseTex, sampler_NoiseTex);

	//Prefer high precision depth
	//#pragma fragmentoption ARB_precision_hint_nicest

	float4 _CloudParams;
	uniform float4x4 clipToWorld;

	struct v2f {
		float4 vertex : SV_POSITION;
		float2 uv : TEXCOORD0;
		float3 worldDirection : TEXCOORD2;
	};

	v2f Vert(AttributesDefault v) {
		v2f o;
		o.vertex = float4(v.vertex.xy, 0.0, 1.0);
		o.uv.xy = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
		o.uv = o.uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
		float4 clip = float4(o.uv.xy * 2 - 1, 0.0, 1.0);
		o.worldDirection.rgb = (mul(clipToWorld, clip.rgba) - _WorldSpaceCameraPos.rgb).xyz;

		//UNITY_SINGLE_PASS_STEREO
		o.uv = TransformStereoScreenSpaceTex(o.uv, 1.0);

		return o;
	}

	float4 Frag(v2f i) : SV_Target
	{
		half4 sceneColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
		float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv);
		float3 worldPos = i.worldDirection * LinearEyeDepth(depth) + _WorldSpaceCameraPos;

		float2 uv = worldPos.xz * _CloudParams.x + (_Time.y * float2(_CloudParams.y, _CloudParams.z));
		float clouds = 1-SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv).a;

		//Clip skybox
		if (Linear01Depth(depth) > 0.99) clouds = 1;

		clouds = lerp(1, clouds, _CloudParams.w);

		float3 cloudsBlend = sceneColor.rgb * clouds;

		return float4(cloudsBlend.rgb, sceneColor.a);
	}


	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			ENDHLSL
		}
	}
}