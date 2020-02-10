Shader "Custom/FogOfWar"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "black" {}
		_TerTex("TerrainTexture", 2D) = "black" {}
		_TeamColor("TeamColor", Color) = (1,1,1,1)
	}
		SubShader
		{
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite On
			Cull Off

			CGPROGRAM

			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows alpha:fade

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _MainTex;
			sampler2D _TerTex;

			struct Input
			{
				float2 uv_MainTex;
				float2 uv_TerTex;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;


			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			//normpdf function gives us a Guassian distribution for each blur iteration; 
			//this is equivalent of multiplying by hard #s 0.16,0.15,0.12,0.09, etc. in code above
			float normpdf(float x, float sigma)
			{
				//return 0.1 * exp(-0.5 * x * x / (sigma * sigma)) / sigma;
				//return 0.39894 * exp( 0.5 * x * x / (sigma * sigma)) / sigma;
				return 0.39894 * exp( 0.5 * x * x / (sigma * sigma)) / sigma;
			}
	
			//this is the blur function... pass in standard col derived from tex2d(_MainTex,i.uv)
			half4 blur(sampler2D tex, float2 uv, float blurAmount) 
			{
				//get our base color...
				half4 col = tex2D(tex, uv);
				//total width/height of our blur "grid":
				const int mSize = 20;
				//this gives the number of times we'll iterate our blur on each side 
				//(up,down,left,right) of our uv coordinate;
				//NOTE that this needs to be a const or you'll get errors about unrolling for loops
				const int iter = (mSize - 1) / 2;
				//run loops to do the equivalent of what's written out line by line above
				//(number of blur iterations can be easily sized up and down this way)
				for (int i = -iter; i <= iter; ++i)
				{
					for (int j = -iter; j <= iter; ++j)
					{
						col += tex2D(tex, float2(uv.x + (i) /** blurAmount*/, uv.y + (j) /** blurAmount*/)) * normpdf(float(i), 7);
					}
	
				}
				//return blurred color
				return col / mSize;
			}
	
			float Outline(Input IN){
				//13,1 13,2 13,3 13,4 13,5 12,6 12,7 11,8 10,9
			//	float points[9] = {12, 12, 12, 12, 12, 11, 11, 10, 9};
				float points[18] = {25,25,25,25,25,25,25,25,24,24,24,23,23,22,21,21,20,19};
	
	
				if(tex2D(_TerTex, IN.uv_TerTex).r == 0.0) {
					return 1.0;
				}
	
				for(int i = 0; i < 18; i++) {
					if(tex2D(_TerTex,  IN.uv_TerTex + (float2(points[i], i))).r == 0.0) {
						return 1.0;
					}
	
					if(tex2D(_TerTex, IN.uv_TerTex + (float2(points[i], -i))).r == 0.0) {
						return 1.0;
					}
	
					if(tex2D(_TerTex, IN.uv_TerTex + (float2(-points[i], i))).r == 0.0) {
						return 1.0;
					}
	
					if(tex2D(_TerTex, IN.uv_TerTex + (-float2(-points[i], -i))).r == 0.0) {
						return 1.0;
					}
	
					if(tex2D(_TerTex, IN.uv_TerTex + (float2(i, points[i]))).r ==0.0) {
						return 1.0;
					}
	
					if(tex2D(_TerTex, IN.uv_TerTex + (float2(i, -points[i]))).r == 0.0) {
						return 1.0;
					}
	
					if(tex2D(_TerTex, IN.uv_TerTex + (float2(-i, points[i]))).r ==0.0) {
						return 1.0;
					}
	
					if(tex2D(_TerTex, IN.uv_TerTex + (float2(-i, -points[i]))).r == 0.0) {
						return 1.0;
					}
				}
	
				return 0.0;
			}

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				fixed4 TerMap = tex2D(_TerTex, IN.uv_TerTex);

				//o.Albedo = c.rgb;
				//	int range = 1;
				// Get the colors of the surrounding pixels
				//fixed3 up = tex2D(_TerTex, IN.uv_TerTex + fixed2(0, range));
				//fixed3 down = tex2D(_TerTex, IN.uv_TerTex - fixed2(0, range));
				//fixed3 left = tex2D(_TerTex, IN.uv_TerTex - fixed2(range, 0));
				//fixed3 right = tex2D(_TerTex, IN.uv_TerTex + fixed2(range, 0));
				//
				//
				//
				//
				//if ((up.r + down.r + left.r + right.r) < 4)
				//{
				//	TerMap.rgb = float3(0,0,0);
				//}

				//float isNotOutline = up * down * left * right;
				//c.rgb = isNotOutline * c.rgb + (1 - isNotOutline) * float3(0, 0, 0);

				//float grayOrginal = (TerMap.r + TerMap.g + TerMap.b) * 0.333;
				//o.Alpha = grayOrginal;

				//float4 col = blur(_TerTex, IN.uv_TerTex, 0.001);
				//float gray = ((col.r + col.g + col.b)) * 0.333;
				//float grayTeamColor = (_TeamColor.r + _TeamColor.g + _TeamColor.b) / 3;

				//float3 delta = abs(TerMap.rgb - _TeamColor.rgb);
				//float bw = length(delta) < 0.1 ? 0 : 1;
				//float bw = length(delta) < 0.3 ? 0 : 1;


				o.Albedo = c.rgb;
			//	o.Albedo =	 Outline(IN);
				//o.Albedo = gray;
				//o.Albedo = col;
			//	o.Albedo = TerMap.rgb;

				o.Alpha = TerMap.rgb;
			//	o.Alpha = Outline(IN);
			//	o.Alpha =   Outline(IN);
			//	o.Alpha = 1;

				//o.Alpha = smoothstep(1, 0, gray);
				//o.Alpha = lerp(0, 1, gray);
				//o.Albedo = c.rgb;

				//o.Albedo = float3(1,0,0);
				//float alpha = smoothstep(1, 0, gray);

			}
			ENDCG
		}
			FallBack "Diffuse"
}
