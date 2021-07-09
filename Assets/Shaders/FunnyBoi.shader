Shader "Skybox/Funny Boi" {
    Properties {
        _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
        [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Rotation", Range(0, 360)) = 0
        [NoScaleOffset] _FrontTex ("Front [+Z]   (HDR)", 2D) = "grey" {}
        [NoScaleOffset] _BackTex ("Back [-Z]   (HDR)", 2D) = "grey" {}
        [NoScaleOffset] _LeftTex ("Left [+X]   (HDR)", 2D) = "grey" {}
        [NoScaleOffset] _RightTex ("Right [-X]   (HDR)", 2D) = "grey" {}
        [NoScaleOffset] _UpTex ("Up [+Y]   (HDR)", 2D) = "grey" {}
        [NoScaleOffset] _DownTex ("Down [-Y]   (HDR)", 2D) = "grey" {}

        [Header(Dither parameters)]
        _DitherPattern ("Dithering Pattern (_DitherPattern)", 2D) = "white" {}
        _Color1 ("Dither tint 1 (_Color1)", Color) = (0, 0, 0, 1)
        _Color2 ("Dithe tint 2 (_Color2)", Color) = (1, 1, 1, 1)
		_Blend ("Blending of tint colors and texture color (_Blend)", Range(0, 1)) = 0
		_DitherStrength("Dither strength multiplier (_DitherStrength)", Float) = 8.0
    }

    SubShader {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        CGINCLUDE
        #include "UnityCG.cginc"

        half4 _Tint;
        half _Exposure;
        float _Rotation;

        //The dithering pattern
        sampler2D _DitherPattern;
        float4 _DitherPattern_TexelSize;

        //Dither colors
        float4 _Color1;
        float4 _Color2;
        float _Blend;
        float _DitherStrength;

        float3 RotateAroundYInDegrees (float3 vertex, float degrees)
        {
            float alpha = degrees * UNITY_PI / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 m = float2x2(cosa, -sina, sina, cosa);
            return float3(mul(m, vertex.xz), vertex.y).xzy;
        }

        struct appdata_t {
            float4 vertex : POSITION;
            float2 texcoord : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        struct v2f {
            float4 vertex : SV_POSITION;
            float2 texcoord : TEXCOORD0;
            float4 screenPosition : TEXCOORD1;
            UNITY_VERTEX_OUTPUT_STEREO
        };
        v2f vert (appdata_t v)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
            o.vertex = UnityObjectToClipPos(rotated);
            o.texcoord = v.texcoord;
            o.screenPosition = ComputeScreenPos(o.vertex);
            
            return o;
        }
        half4 skybox_frag (v2f i, sampler2D smp, half4 smpDecode)
        {
            half4 tex = tex2D (smp, i.texcoord);
            half3 col = DecodeHDR (tex, smpDecode);
            col = col * _Tint.rgb * unity_ColorSpaceDouble.rgb;
            col *= _Exposure;

            float3 c = col.xyz * 255.0; //extrapolate 16bit color float to 16bit integer space
            float2 screenPos = i.screenPosition.xy / i.screenPosition.w;
            float2 ditherCoordinate = screenPos * _ScreenParams.xy * _DitherPattern_TexelSize.xy;
            float dither = tex2D(_DitherPattern, ditherCoordinate).r;
            // int dither = psx_dither_table[screenPos.x % 4][screenPos.y % 4];
            c += (dither * _DitherStrength - 4.0); //dithering process as described in PSYDEV SDK documentation
            c = lerp((uint3(c) & 0xf8), 0xf8, step(0xf8,c)); 
            //truncate to 5bpc precision via bitwise AND operator, and limit value max to prevent wrapping.
            //PS1 colors in default color mode have a maximum integer value of 248 (0xf8)
            c /= 255; //bring color back to floating point number space
            return float4(c.r, c.g, c.b, tex.a);
        }
        ENDCG

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            sampler2D _FrontTex;
            half4 _FrontTex_HDR;
            half4 frag (v2f i) : SV_Target { return skybox_frag(i,_FrontTex, _FrontTex_HDR); }
            ENDCG
        }
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            sampler2D _BackTex;
            half4 _BackTex_HDR;
            half4 frag (v2f i) : SV_Target { return skybox_frag(i,_BackTex, _BackTex_HDR); }
            ENDCG
        }
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            sampler2D _LeftTex;
            half4 _LeftTex_HDR;
            half4 frag (v2f i) : SV_Target { return skybox_frag(i,_LeftTex, _LeftTex_HDR); }
            ENDCG
        }
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            sampler2D _RightTex;
            half4 _RightTex_HDR;
            half4 frag (v2f i) : SV_Target { return skybox_frag(i,_RightTex, _RightTex_HDR); }
            ENDCG
        }
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            sampler2D _UpTex;
            half4 _UpTex_HDR;
            half4 frag (v2f i) : SV_Target { return skybox_frag(i,_UpTex, _UpTex_HDR); }
            ENDCG
        }
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            sampler2D _DownTex;
            half4 _DownTex_HDR;
            half4 frag (v2f i) : SV_Target { return skybox_frag(i,_DownTex, _DownTex_HDR); }
            ENDCG
        }
    }
}