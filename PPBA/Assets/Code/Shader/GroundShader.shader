Shader "Custom/GroundWithResourceCalc"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		[Toggle(MetalInt)]
		_MetalResourcesInt("MetalInt", Int) = 1.0

		_ResourceMap("ResourceMap (RGB)", 2D) = "white" {}
		_NoiseMap("Noise (RGB)", 2D) = "white" {}
		_Position("World Position",Vector) = (0,0,0,0)
		_Radius("Sphere Radius", Range(0,100))= 0
		_Softness("Sphere Softness", Range(0,100)) = 0
		_ColorResource("ColorResource", Color) = (1,1,1,1)
		_valueIntensity("ValueIntensity", int) = 0
	}

		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _MainTex;
			sampler2D _ResourceMap;
			sampler2D _NoiseMap;

			struct Input
			{
				float2 uv_MainTex;
				float2 uv_ResourceMap;
				float2 uv_NoiseMap;
				float3 worldPos;
			};

			int _valueIntensity;
			half _Glossiness;
			half _Metallic;
			fixed4 _Color;
			fixed4 _ColorResource;
			int _MetalResourcesInt;

			// Sphere Mask
			float4 _Position;
			half _Radius;
			half _Softness;


			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				// Albedo comes from a texture tinted by color
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				fixed4 map = tex2D(_ResourceMap, IN.uv_ResourceMap) * _Color;
				fixed4 noise = tex2D(_NoiseMap, IN.uv_NoiseMap);
				
				int i = step(_MetalResourcesInt,0);


			//	noise.r +=  _valueIntensity;
				fixed4 z = lerp(_ColorResource,map,_valueIntensity);

				fixed4 t = lerp(_ColorResource, z, noise.r);

				//half d = distance(_Position, IN.worldPos);
				//half sum = saturate((d - _Radius) / -_Softness);
				//fixed4 lerpColor = lerp(_ColorResource,t,sum);

				fixed4 endCol = lerp(c, t,i);
				o.Albedo = endCol.rgb;
				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = t.a;
			}
			ENDCG
		}
			FallBack "Diffuse"
}
