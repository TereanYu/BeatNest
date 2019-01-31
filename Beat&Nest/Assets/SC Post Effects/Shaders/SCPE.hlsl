// SC Post Effects
// Staggart Creations
// http://staggart.xyz

float2 RotateUV(float2 uv, float rotation) {
	float cosine = cos(rotation);
	float sine = sin(rotation);
	float2 pivot = float2(0.5, 0.5);
	float2 rotator = (mul(uv - pivot, float2x2(cosine, -sine, sine, cosine)) + pivot);
	return saturate(rotator);
}

float3 ChromaticAberration(TEXTURE2D_ARGS(tex, samplerTex), float4 texelSize, float2 uv, float amount) {
		float2 direction = normalize((float2(0.5, 0.5) - uv));
		float3 distortion = float3(-texelSize.x * amount, 0, texelSize.x * amount);

		float red = SAMPLE_TEXTURE2D(tex, samplerTex, uv + direction * distortion.r).r;
		float green = SAMPLE_TEXTURE2D(tex, samplerTex, uv + direction * distortion.g).g;
		float blue = SAMPLE_TEXTURE2D(tex, samplerTex, uv + direction * distortion.b).b;

		return float3(red, green, blue);
	}

float4 LuminanceThreshold(float4 color, float threshold)
{
	return max(color - threshold, 0);
}

/*
float3 PositionFromDepth(float depth, float2 uv, float4 inverseViewMatrix) {

	float4 clip = float4((uv.xy * 2.0f - 1.0f) * float2(1, -1), 0.0f, 1.0f);
	float3 worldDirection = mul(inverseViewMatrix, clip) - _WorldSpaceCameraPos;

	float3 worldspace = worldDirection * depth + _WorldSpaceCameraPos;

	return float3(frac((worldspace.rgb)) + float3(0, 0, 0.1));
}
*/

// (returns 1.0 when orthographic)
float CheckPerspective(float x)
{
	return lerp(x, 1.0, unity_OrthoParams.w);
}

// Reconstruct view-space position from UV and depth.
float3 ReconstructViewPos(float2 uv, float depth)
{
	float3 worldPos = float3(0, 0, 0);
	worldPos.xy = (uv.xy * 2.0 - 1.0 - float2(unity_CameraProjection._13, unity_CameraProjection._23)) / float2(unity_CameraProjection._11, unity_CameraProjection._22) * CheckPerspective(depth);
	worldPos.z = depth;
	return worldPos;
}

float2 Distort(float2 uv)
{
#if DISTORT
	{
		uv = (uv - 0.5) * _Distortion_Amount.z + 0.5;
		float2 ruv = _Distortion_CenterScale.zw * (uv - 0.5 - _Distortion_CenterScale.xy);
		float ru = length(float2(ruv));

		UNITY_BRANCH
			if (_Distortion_Amount.w > 0.0)
			{
				float wu = ru * _Distortion_Amount.x;
				ru = tan(wu) * (1.0 / (ru * _Distortion_Amount.y));
				uv = uv + ruv * (ru - 1.0);
			}
			else
			{
				ru = (1.0 / ru) * _Distortion_Amount.x * atan(ru * _Distortion_Amount.y);
				uv = uv + ruv * (ru - 1.0);
			}
	}
#endif

	return uv;
}

float2 FisheyeUV(half2 uv, half amount, half zoom)
{
	half2 center = uv.xy - half2(0.5, 0.5);
	half CdotC = dot(center, center);
	half f = 1.0 + CdotC * (amount * sqrt(CdotC));
	return f * zoom * center + 0.5;
}