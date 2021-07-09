Shader "My Shaders/Skybox Shader"{
    //show values to edit in inspector
    Properties{
        [Header(Skybox parameters)]
        _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
        [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Rotation", Range(0, 360)) = 0
        [NoScaleOffset] _MainTex ("Spherical  (HDR)", 2D) = "grey" {}
        [KeywordEnum(6 Frames Layout, Latitude Longitude Layout)] _Mapping("Mapping", Float) = 1
        [Enum(360 Degrees, 0, 180 Degrees, 1)] _ImageType("Image Type", Float) = 0
        [Toggle] _MirrorOnBack("Mirror on Back", Float) = 0
        [Enum(None, 0, Side by Side, 1, Over Under, 2)] _Layout("3D Layout", Float) = 0

        [Header(Dither parameters)]
        _DitherPattern ("Dithering Pattern", 2D) = "white" {}
        _Color1 ("Dither Color 1", Color) = (0, 0, 0, 1)
        _Color2 ("Dither Color 2", Color) = (1, 1, 1, 1)
		_Blend ("Blending of color 2 and texture color", Range(0, 1)) = 0

		[Header(Random Noise parameters)]
        _NoiseTex ("Noise Texture", 2D) = "white" {}
		_RandomNoiseCutoff ("Cutoff value for random noise", Range(0, 1.5)) = 0

		[Header(Perlin Noise parameters)]
		_CellSize ("Cell Size", Range(100, 5000)) = 1
		_ScrollSpeed ("Scroll Speed", Range(0, 1)) = 1
        _UseHeightLine ("Use height line (0 or 1)", int) = 1
        _HeightLineSize ("Height line size", Range(0, 50)) = 1
		_PerlinNoiseCutoff ("Cutoff value for perlin noise", Range(0, 1.5)) = 0
    }

    SubShader {
        //the material is completely non-transparent and is rendered at the same time as the other opaque geometry
        Tags{ "RenderType"="Opaque" "Queue"="Geometry"}
        Cull Off ZWrite Off

        Pass {

            CGPROGRAM

            //include useful shader functions
            #include "UnityCG.cginc"

            //define vertex and fragment shader
            #pragma vertex vert
            #pragma fragment frag

            // ---variable declarations---

            //The dithering pattern
            sampler2D _DitherPattern;
            float4 _DitherPattern_TexelSize;

            //Random noise vars
            uniform sampler2D _NoiseTex;
            float _RandomNoiseCutoff;

            //Perlin noise vars
            float _CellSize;
            float _ScrollSpeed;
            float _PerlinNoiseCutoff;
            int _UseHeightLine;
            float _HeightLineSize;

            //skybox vars
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            half4 _MainTex_HDR;
            half4 _Tint;
            half _Exposure;
            float _Rotation;
            bool _MirrorOnBack;
            int _ImageType;
            int _Layout;

            //Dither colors
            float4 _Color1;
            float4 _Color2;
            float _Blend;
            
            // --- skybox functions ---

            inline float2 ToCubeCoords(float3 coords, float3 layout, float4 edgeSize, float4 faceXCoordLayouts, float4 faceYCoordLayouts, float4 faceZCoordLayouts)
            {
                // Determine the primary axis of the normal
                float3 absn = abs(coords);
                float3 absdir = absn > float3(max(absn.y,absn.z), max(absn.x,absn.z), max(absn.x,absn.y)) ? 1 : 0;
                // Convert the normal to a local face texture coord [-1,+1], note that tcAndLen.z==dot(coords,absdir)
                // and thus its sign tells us whether the normal is pointing positive or negative
                float3 tcAndLen = mul(absdir, float3x3(coords.zyx, coords.xzy, float3(-coords.xy,coords.z)));
                tcAndLen.xy /= tcAndLen.z;
                // Flip-flop faces for proper orientation and normalize to [-0.5,+0.5]
                bool2 positiveAndVCross = float2(tcAndLen.z, layout.x) > 0;
                tcAndLen.xy *= (positiveAndVCross[0] ? absdir.yx : (positiveAndVCross[1] ? float2(absdir[2],0) : float2(0,absdir[2]))) - 0.5;
                // Clamp values which are close to the face edges to avoid bleeding/seams (ie. enforce clamp texture wrap mode)
                tcAndLen.xy = clamp(tcAndLen.xy, edgeSize.xy, edgeSize.zw);
                // Scale and offset texture coord to match the proper square in the texture based on layout.
                float4 coordLayout = mul(float4(absdir,0), float4x4(faceXCoordLayouts, faceYCoordLayouts, faceZCoordLayouts, faceZCoordLayouts));
                tcAndLen.xy = (tcAndLen.xy + (positiveAndVCross[0] ? coordLayout.xy : coordLayout.zw)) * layout.yz;
                return tcAndLen.xy;
            }        
            
            float3 RotateAroundYInDegrees (float3 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }
            
            // --- noise funcitons ---

            float4 getNoise(float2 uv) 
            {
                float4 noise = tex2D(_NoiseTex, 10000 + uv + _Time * 50);
                // noise = mad(noise, 2.0, -0.5);
                return noise;
            }

            float rand3dTo1d(float3 value, float3 dotDir = float3(12.9898, 78.233, 37.719)){
                //make value smaller to avoid artefacts
                float3 smallValue = sin(value);
                //get scalar value from 3d vector
                float random = dot(smallValue, dotDir);
                //make value more random by making it bigger and then taking teh factional part
                random = frac(sin(random) * 143758.5453);
                return random;
            }

            float3 rand3dTo3d(float3 value){
                return float3(
                    rand3dTo1d(value, float3(12.989, 78.233, 37.719)),
                    rand3dTo1d(value, float3(39.346, 11.135, 83.155)),
                    rand3dTo1d(value, float3(73.156, 52.235, 09.151))
                );
            }

            inline float easeIn(float interpolator){
                return interpolator * interpolator;
            }

            float easeOut(float interpolator){
                return 1 - easeIn(1 - interpolator);
            }

            float easeInOut(float interpolator){
                float easeInValue = easeIn(interpolator);
                float easeOutValue = easeOut(interpolator);
                return lerp(easeInValue, easeOutValue, interpolator);
            }

            float perlinNoise(float3 value){
                float3 fraction = frac(value);

                float interpolatorX = easeInOut(fraction.x);
                float interpolatorY = easeInOut(fraction.y);
                float interpolatorZ = easeInOut(fraction.z);

                float3 cellNoiseZ[2];
                [unroll]
                for(int z=0;z<=1;z++){
                    float3 cellNoiseY[2];
                    [unroll]
                    for(int y=0;y<=1;y++){
                        float3 cellNoiseX[2];
                        [unroll]
                        for(int x=0;x<=1;x++){
                            float3 cell = floor(value) + float3(x, y, z);
                            float3 cellDirection = rand3dTo3d(cell) * 2 - 1;
                            float3 compareVector = fraction - float3(x, y, z);
                            cellNoiseX[x] = dot(cellDirection, compareVector);
                        }
                        cellNoiseY[y] = lerp(cellNoiseX[0], cellNoiseX[1], interpolatorX);
                    }
                    cellNoiseZ[z] = lerp(cellNoiseY[0], cellNoiseY[1], interpolatorY);
                }
                float3 noise = lerp(cellNoiseZ[0], cellNoiseZ[1], interpolatorZ);
                return noise;
            }

            //input struct which is automatically filled by unity
            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            //the data that's used to generate fragments and can be read by the fragment shader
            struct v2f{
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
                float3 layout : TEXCOORD1;
                float4 edgeSize : TEXCOORD2;
                float4 faceXCoordLayouts : TEXCOORD3;
                float4 faceYCoordLayouts : TEXCOORD4;
                float4 faceZCoordLayouts : TEXCOORD5;

                float4 screenPosition : TEXCOORD6;
                float4 worldPosition : TEXCOORD7;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
                o.vertex = UnityObjectToClipPos(rotated);
                o.texcoord = v.vertex.xyz;
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex) ;
                o.screenPosition = ComputeScreenPos(o.worldPosition);
                // mul(unity_ObjectToWorld, vertex);
                // layout and edgeSize are solely based on texture dimensions and can thus be precalculated in the vertex shader.
                float sourceAspect = float(_MainTex_TexelSize.z) / float(_MainTex_TexelSize.w);
                // Use the halfway point between the 1:6 and 3:4 aspect ratios of the strip and cross layouts to
                // guess at the correct format.
                bool3 aspectTest =
                    sourceAspect >
                    float3(1.0, 1.0f / 6.0f + (3.0f / 4.0f - 1.0f / 6.0f) / 2.0f, 6.0f / 1.0f + (4.0f / 3.0f - 6.0f / 1.0f) / 2.0f);
                // For a given face layout, the coordinates of the 6 cube faces are fixed: build a compact representation of the
                // coordinates of the center of each face where the first float4 represents the coordinates of the X axis faces,
                // the second the Y, and the third the Z. The first two float componenents (xy) of each float4 represent the face
                // coordinates on the positive axis side of the cube, and the second (zw) the negative.
                // layout.x is a boolean flagging the vertical cross layout (for special handling of flip-flops later)
                // layout.yz contains the inverse of the layout dimensions (ie. the scale factor required to convert from
                // normalized face coords to full texture coordinates)
                if (aspectTest[0]) // horizontal
                {
                    if (aspectTest[2])
                    { // horizontal strip
                        o.faceXCoordLayouts = float4(0.5,0.5,1.5,0.5);
                        o.faceYCoordLayouts = float4(2.5,0.5,3.5,0.5);
                        o.faceZCoordLayouts = float4(4.5,0.5,5.5,0.5);
                        o.layout = float3(-1,1.0/6.0,1.0/1.0);
                    }
                    else
                    { // horizontal cross
                        o.faceXCoordLayouts = float4(2.5,1.5,0.5,1.5);
                        o.faceYCoordLayouts = float4(1.5,2.5,1.5,0.5);
                        o.faceZCoordLayouts = float4(1.5,1.5,3.5,1.5);
                        o.layout = float3(-1,1.0/4.0,1.0/3.0);
                    }
                }
                else
                {
                    if (aspectTest[1])
                    { // vertical cross
                        o.faceXCoordLayouts = float4(2.5,2.5,0.5,2.5);
                        o.faceYCoordLayouts = float4(1.5,3.5,1.5,1.5);
                        o.faceZCoordLayouts = float4(1.5,2.5,1.5,0.5);
                        o.layout = float3(1,1.0/3.0,1.0/4.0);
                    }
                    else
                    { // vertical strip
                        o.faceXCoordLayouts = float4(0.5,5.5,0.5,4.5);
                        o.faceYCoordLayouts = float4(0.5,3.5,0.5,2.5);
                        o.faceZCoordLayouts = float4(0.5,1.5,0.5,0.5);
                        o.layout = float3(-1,1.0/1.0,1.0/6.0);
                    }
                }
                // edgeSize specifies the minimum (xy) and maximum (zw) normalized face texture coordinates that will be used for
                // sampling in the texture. Setting these to the effective size of a half pixel horizontally and vertically
                // effectively enforces clamp mode texture wrapping for each individual face.
                o.edgeSize.xy = _MainTex_TexelSize.xy * 0.5 / o.layout.yz - 0.5;
                o.edgeSize.zw = -o.edgeSize.xy;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 tc = ToCubeCoords(i.texcoord, i.layout, i.edgeSize, i.faceXCoordLayouts, i.faceYCoordLayouts, i.faceZCoordLayouts);
                half4 tex = tex2D (_MainTex, tc);
                half3 c = DecodeHDR (tex, _MainTex_HDR);
                c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
                c *= _Exposure;

                float texColor = half4(c, 1);

                //value from the dither pattern
                float2 screenPos = i.screenPosition.xy / i.screenPosition.w;
                float2 ditherCoordinate = screenPos * _ScreenParams.xy * _DitherPattern_TexelSize.xy;
                float ditherValue = tex2D(_DitherPattern, ditherCoordinate).r;
                
                //apply noise to dither pattern value
                float noise = getNoise(i.texcoord) * 1.5;
                if(noise + ditherValue <= _RandomNoiseCutoff) {
                    noise = mad(noise, 2.0, -0.5);
                    ditherValue += noise;
                }

                //calculate perlin noise values
                float3 pValue = i.worldPosition / _CellSize;
                pValue.x += _Time.x * _ScrollSpeed * 20;
                pValue.y += _Time.y * _ScrollSpeed;
                pValue.z += _Time.z * _ScrollSpeed / 2;
    
                //get noise and adjust it to be ~0-1 range
                float pNoise = perlinNoise(pValue) + 0.5;
                pNoise = frac(pNoise * 6);

                if(_UseHeightLine == 1)
                {
                    float pixelNoiseChange = fwidth(pNoise * _HeightLineSize);
                    float heightLine = smoothstep(1-pixelNoiseChange, 1, pNoise);
                    heightLine += smoothstep(pixelNoiseChange, 0, pNoise);
                    pNoise = heightLine;
                }

                //apply perlin noise to value from dither
                if(ditherValue + pNoise <= _PerlinNoiseCutoff) {
                    ditherValue += pNoise;
                }

                float ditheredValue = step(ditherValue, texColor);

                float4 blendColor2 = lerp(_Color2, texColor, _Blend);
                float4 blendColor1 = lerp(_Color1, texColor, _Blend);
                return lerp(blendColor1, blendColor2, ditheredValue);
            }
            ENDCG
        }
    }
    FallBack Off
}