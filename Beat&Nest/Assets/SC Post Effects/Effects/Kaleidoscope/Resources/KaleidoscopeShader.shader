/*
Copyright(C) 2015 Keijiro Takahashi

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files(the "Software"),
to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
and / or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions :

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

Shader "Hidden/SC Post Effects/Kaleidoscope"
{
	HLSLINCLUDE

#include "../../../Shaders/StdLib.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	float _Splits;
	float _Rotation;

	float4 Frag(VaryingsDefault i) : SV_Target
	{

		// Convert to the polar coordinate.
		float2 sc = i.texcoordStereo - 0.5;
		float phi = atan2(sc.y, sc.x);
		float r = sqrt(dot(sc, sc));

		// Angular repeating.
		phi += _Rotation;
		phi = phi - _Splits * floor(phi / _Splits);
		phi = min(phi, _Splits - phi);

		// Convert back to the texture coordinate.
		float2 uv = float2(cos(phi), sin(phi)) * r + 0.5;

		// Reflection at the border of the screen.
		uv = max(min(uv, 2.0 - uv), -uv);

		return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
	}

		float2 mod(float a, float b)
	{
		return a - floor(b * (1.0 / 289.0)) * 289.0;
	}

#define PI2 6.28318530718
#define DegreeToRad (3.14159/180.0)
#define Tau (3.14159 * 2.0)
#define cos30 (0.8660254) /* sqrt(3)/2 */
#define TILE_TRI 0
#define TILE_SQU 1
#define TILE_HEX 2
#define TILE_OCT 3

#define STYLE_MATCHING 0
#define STYLE_SEAMLESS 1

#define TILE_MAX_SIDES 8

	struct Tile_t
	{
		float2  center;     // Center of the tile
		int   sides;      // Number of sides
		float len;        // Length of the side
		float radius;     // Radius to the vertex
		float inner;      // Radius to closest point on the edge
		float angle;      // Angle to first vertex
		float horzangle;  // Angle between canonical first vertex and horizontal 
		int   direction;  // Rotation direction (+1 or -1)
	};

	/*--------------------------------------------------------------
 * Locate (Equilateral) Triangle Tile
 *
 * Canonical tile: vertex zero at 90 degrees, alternating tiles rotated 180 degrees
 *
 * Styles:
 *   MATCHING
 *   SEAMLESS
 *--------------------------------------------------------------*/
	Tile_t tile_LocateTriangle(float2 aCoord, float radius)
	{
		Tile_t t;

		float sideLen = radius * 2.0 * cos30;

		// Compute the size of a box to contain an equilateral triangle with a side of length=sideLen
		float2 boxSize = float2(sideLen, sideLen * cos30);

		// Determine if this is even or odd row. First convert the vertical location to a row number
		// Determine if it is an odd or even row
		// For odd rows, invert the triangle
		// -- This inverts the results when y<0 -- int row = int(aCoord.y/boxSize.y);
		// -- This inverts the results when y<0 -- bool evenRow = ((row - 2*(row/2)) == 0);
		float row = floor(aCoord.y / boxSize.y);
		bool evenRow = ((row - 2.0*floor(row / 2.0)) < 0.01);

		// Compute the center of the triangle relative to the bottom-left corner of the box
		// Note that triangles are inverted for odd rows, so the center is shifted
		float2 ctrAdjA = float2(boxSize.x * 0.5, boxSize.y * (evenRow ? 1.0 : 2.0) / 3.0);
		float2 coordA = aCoord.xy;
		// Find the box containing the coord, then compute the triangle center
		float2 boxA = floor(coordA / boxSize);
		float2 ctrA = boxA * boxSize + ctrAdjA;
		// Triangles are inverted on odd rows
		float angleA = evenRow ? 90.0 : 270.0;
		int   dirA = 1;

		//Seamless
			int idx = int(boxA.x - 3.0 * floor(boxA.x / 3.0));
			dirA = evenRow ? 1 : -1;
			angleA = 330.0 - float(idx) * 120.0;
			if (!evenRow)
			angleA = -angleA;
		

		// Same as above, but we shift sideways by half a box
		// and invert all of the triangles
		float2 shiftB = float2(boxSize.x * 0.5, 0.0);
		float2 ctrAdjB = float2(boxSize.x * 0.5, boxSize.y * (evenRow ? 2.0 : 1.0) / 3.0);
		float2 coordB = aCoord.xy + shiftB;
		float2 boxB = floor((coordB) / boxSize);
		float2 ctrB = boxB * boxSize - shiftB + ctrAdjB;
		float angleB = evenRow ? 270.0 : 90.0;
		int   dirB = 1;

		//Seamless
			idx = int(boxB.x - 3.0 * floor((boxB.x) / 3.0));
			dirB = evenRow ? -1 : 1;
			angleB = 150.0 + float(idx) * 120.0;
			if (!evenRow)
				angleB = -angleB;
		

		bool chooseA = (distance(aCoord, ctrA) < distance(aCoord, ctrB));
		float2 ctr = (chooseA) ? ctrA : ctrB;
		float angle = (chooseA) ? angleA : angleB;
		int   dir = (chooseA) ? dirA : dirB;

		t.center = ctr;
		t.len = sideLen;
		t.sides = 3;
		t.radius = radius;
		t.inner = sideLen / 4.0;
		t.angle = angle;
		t.horzangle = -90.0;
		t.direction = dir;


		return t;
	}

	/*--------------------------------------------------------------
 * Locate dispatch routine
 *--------------------------------------------------------------*/
	Tile_t tile_Locate(float2 aCoord, float radius)
	{
		Tile_t t;

		t = tile_LocateTriangle(aCoord, radius);

		return t;
	}

	/*--------------------------------------------------------------
 * Calculate the position of a coordinate relative to vertex 0,
 * taking into consideration the direction of the tile.
 * The result is -radius<=x<=radius, -radius<=y<=radius
 *--------------------------------------------------------------*/
	float2 tile_CalcRelPosition(Tile_t tile, float2 coord, float twist)
	{
		float2 relPos;

		float angle = -(tile.angle + tile.horzangle + twist * float(tile.direction)) *  DegreeToRad;

		float cA = cos(angle);
		float sA = sin(angle);

		float4 rm = float4(cA, sA, -sA, cA);

		relPos = coord - tile.center;

		relPos = rm * relPos;

		if (tile.direction == -1)
			relPos.x = -relPos.x;
		return relPos;
	}

	/*--------------------------------------------------------------
 * Calculate the relative position, but return values appropriate
 * for a texture lookup.
 *--------------------------------------------------------------*/
	float2 tile_CalcRelPositionUV(Tile_t tile, float2 coord, float twist)
	{
		float2 relPos = tile_CalcRelPosition(tile, coord, twist);

		float2 uv = (relPos + float2(tile.radius, tile.radius)) / (tile.radius * 2.0);

		return uv;
	}

	float4 FragNew(VaryingsDefault i) : SV_Target
	{
		// x position determines size
		float size = 0.1;
		float2 uv = i.texcoordStereo;

		// Locate a tile
		Tile_t tile = tile_Locate(uv.xy, size);

		// Convert coordinate to a relative position in the tile
		uv = tile_CalcRelPositionUV(tile, i.texcoord.xy, 10.0);

		return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
	}

		ENDHLSL

		SubShader
	{
		Cull Front ZWrite Off ZTest Always

			Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment Frag

			ENDHLSL
		}
	}
}