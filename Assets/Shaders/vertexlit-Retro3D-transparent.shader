// mostly from https://github.com/dsoft20/psx_retroshader
// mixed with parts of https://github.com/keijiro/Retro3D
// under MIT license by Robert Yang (https://debacle.us)

Shader "psx/VertexLit-Retro3D transparent" {
	Properties{
		[Header(Retro3D parameters)]
		_MainTex("Main texture (_MainTex)", 2D) = "white" {}
		_Color("Tint (_Color)", Color) = (0.5, 0.5, 0.5, 1)
		_GeoRes("Geometric resolution (_GeoRes)", Float) = 40
    	_ColorQuality("Color quality (_ColorQuality)", Int) = 8
    	_AffineStrength("Affine UV mapping strength (_AffineStrength)", Range(0, 1)) = 1
		_CutoffDistance("Polygon cutoff distance (_CutoffDistance)", Float) = 100

        [Header(Dither parameters)]
        _DitherPattern ("Dithering Pattern (_DitherPattern)", 2D) = "white" {}
        _Color1 ("Dither tint 1 (_Color1)", Color) = (0, 0, 0, 1)
        _Color2 ("Dither tint 2 (_Color2)", Color) = (1, 1, 1, 1)
		_Blend ("Blending of tint colors and texture color (_Blend)", Range(0, 1)) = 0
		_DitherStrength("Dither strength multiplier (_DitherStrength)", Float) = 8.0
	}
		SubShader {
			Tags { "LightMode" = "Vertex" "Queue" = "Transparent" "RenderType"="Transparent"}
			LOD 200

			Pass {
			Lighting On
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f
			{
				fixed4 pos : SV_POSITION;
				half4 color : COLOR0;
				half4 colorFog : COLOR1;
				float2 uv_MainTex : TEXCOORD0;
				half3 normal : TEXCOORD1;
                int cutoffValue : TEXCOORD2;
            	float4 screenPosition : TEXCOORD3;
			};

			sampler2D _MainTex;
            float4 _MainTex_TexelSize;
			float4 _MainTex_ST;
			uniform half4 unity_FogStart;
			uniform half4 unity_FogEnd;

			// 3D shit
			float4 _Color;
			float _GeoRes;
			int _ColorQuality;
			float _AffineStrength;
			float _CutoffDistance;

			//The dithering pattern
			sampler2D _DitherPattern;
			float4 _DitherPattern_TexelSize;

			//Dither colors
			float4 _Color1;
			float4 _Color2;
			float _Blend;
			
			float _DitherStrength;

			float rand3dTo1d(float3 value, float3 dotDir = float3(12.9898, 78.233, 37.719))
			{
				//make value smaller to avoid artefacts
				float3 smallValue = sin(value);
				//get scalar value from 3d vector
				float random = dot(smallValue, dotDir);
				//make value more random by making it bigger and then taking teh factional part
				random = frac(sin(random) * 143758.5453);
				return random - 0.5;
			}

			v2f vert(appdata_full v)
			{
				v2f o;

				// including the line below will tell Unity not to upgrade the matrix mul() operations below...
				// UNITY_SHADER_NO_UPGRADE
				float4 wp = mul(UNITY_MATRIX_MV, v.vertex);
				// wp.xyz = floor(wp.xyz * _GeoRes) / _GeoRes;
				float3 displacement = wp.xyz - floor(wp.xyz * _GeoRes) / _GeoRes ;
				displacement *= sign(rand3dTo1d(wp.xyz));
				displacement /= 3;
				wp.xyz += displacement;

				float4 sp = mul(UNITY_MATRIX_P, wp);
				o.pos = sp;

				//Vertex lighting 
				o.color = float4(ShadeVertexLightsFull(v.vertex, v.normal, 4, true), 1.0);
				o.color *= v.color; // vertex color support

				float distance = length(UnityObjectToClipPos(v.vertex));

				//Affine Texture Mapping
				float4 affinePos = wp;				
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv_MainTex *= 1 + _AffineStrength * (distance + (wp.w*(UNITY_LIGHTMODEL_AMBIENT.a * 16)) / distance / 2);
				o.normal = 1 + _AffineStrength * (distance + (wp.w*(UNITY_LIGHTMODEL_AMBIENT.a * 16)) / distance / 2);

				//Fog
				float4 fogColor = unity_FogColor;
				float fogDensity = (unity_FogEnd - distance) / (unity_FogEnd - unity_FogStart);
				o.normal.g = fogDensity;
				o.normal.b = 1;

				o.colorFog = fogColor;
				o.colorFog.a = clamp(fogDensity,0,1);

                // Cut out polygons
				// cutoffValue should be 1 if distance > _CutoffDistance and 0 otherwise
				// round( 0.5 - (distance / _CutoffDistance) ) >= 1 if distance >= _CutoffDistance
					// (distance / _CutoffDistance) - 0.5 >= 0.5 if distance >= _CutoffDistance
					// (distance / _CutoffDistance) - 0.5 < 0.5 if distance < _CutoffDistance
				float cutoffValue = round(distance / _CutoffDistance - 0.5);
                o.cutoffValue = cutoffValue;

				// get screen position for dithering
            	o.screenPosition = ComputeScreenPos(o.pos);

				return o;
			}

			static const float4x4 psx_dither_table = float4x4
			(
				0,    8,    2,    10,
				12,    4,    14,    6, 
				3,    11,    1,    9, 
				15,    7,    13,    5
			);

			float4 frag(v2f i) : COLOR
			{
				// remove pixels from polygons that are out of render distance
                clip(-i.cutoffValue);
				
				// get colors from main texture and fog
				float4 col = tex2D(_MainTex, i.uv_MainTex / i.normal.r) * i.color * _Color;
				float4 color = col*(i.colorFog.a);
				color.rgb += i.colorFog.rgb*(1 - i.colorFog.a);
				color.a = col.a;
				
				// /*
				float3 c = color.xyz * 255.0; //extrapolate 16bit color float to 16bit integer space
                float2 screenPos = i.screenPosition.xy / i.screenPosition.w;
                float2 ditherCoordinate = screenPos * _ScreenParams.xy * _DitherPattern_TexelSize.xy;
                float dither = tex2D(_DitherPattern, ditherCoordinate).r;
				// int dither = psx_dither_table[screenPos.x % 4][screenPos.y % 4];
				c += (dither * _DitherStrength - 4.0); //dithering process as described in PSYDEV SDK documentation
				c = lerp((uint3(c) & 0xf8), 0xf8, step(0xf8,c)); 
				//truncate to 5bpc precision via bitwise AND operator, and limit value max to prevent wrapping.
				//PS1 colors in default color mode have a maximum integer value of 248 (0xf8)
				c /= 255; //bring color back to floating point number space
				return float4(c.r, c.g, c.b, color.a);
				// */

				/*
                // value from the dither pattern
                float2 screenPos = i.screenPosition.xy / i.screenPosition.w;
                float2 ditherCoordinate = screenPos * _ScreenParams.xy * _DitherPattern_TexelSize.xy;
                float ditherTexValue = tex2D(_DitherPattern, ditherCoordinate).r;

				// calculate low and high shades for color and blend them with color1 and color2
				float4 blend1 = lerp(color - color * _DitherStrength / 2 * fwidth(color.r), _Color1, _Blend);
				float4 blend2 = lerp(color + color * _DitherStrength / 2 * fwidth(color.r), _Color2, _Blend);

				// lower color quality
				blend1 = round(blend1 * _ColorQuality) / _ColorQuality;
				blend2 = round(blend2 * _ColorQuality) / _ColorQuality;
				
				// combine dither pattern with texture value to get final result
				float ditherSteppedValue = step(ditherTexValue, color + sign(0.5 - color) * (distance(color, 0.5)) * _DitherStrength);
				float ditheredValue = lerp(ditherTexValue, step(ditherTexValue, ditherSteppedValue), _Blend);

				return lerp(blend1, blend2, ditheredValue);
				*/
			}
			
			ENDCG
		}
	}
}