Shader "Custom/GroundWithResourceCalc"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_ParallaxMap ("Heightmap (A)", 2D) = "black" {}
		_Parallax ("Height", Range (0.005, 0.08)) = 0.02
		_OcclusionMap ("Occlusion", 2D) = "white" {}

		_BumpMap ("Bumpmap", 2D) = "bump" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		[Toggle(MetalInt)]
		_MetalResourcesInt("MetalInt", Int) = 1.0

		// _ResourceMap("ResourceMap (RGB)", 2D) = "white" {}
		_NoiseMap("Noise (RGB)", 2D) = "white" {}
		_Position("World Position",Vector) = (0,0,0,0)
		_Softness("Sphere Softness", Range(0,100)) = 0
		_ColorResource("ColorResource", Color) = (1,1,1,1)
	}

		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows
			#pragma shader_feature _OCCLUSION_MAP
			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _MainTex;
 			sampler2D _ParallaxMap;			// sampler2D _ResourceMap;
			sampler2D _NoiseMap;
			sampler2D _BumpMap;

			sampler2D _OcclusionMap;


			struct Input
			{
				float2 uv_MainTex;
				// float2 uv_ResourceMap;
				float2 uv_NoiseMap;
				float2 uv_BumpMap;
				float2 uv_HeightMap;
				float2 uv_OcclusionMap;
				float3 viewDir;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;
			fixed4 _ColorResource;
			int _MetalResourcesInt;
			half _HeightIntensity;
			float _Parallax;
			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input IN, inout SurfaceOutputStandard o)
			{

				half h = tex2D (_ParallaxMap, IN.uv_BumpMap).w;
     			float2 offset = ParallaxOffset (h, _Parallax, IN.viewDir);

    			IN.uv_MainTex += offset;
     			IN.uv_BumpMap += offset;

				// Albedo comes from a texture tinted by color
				fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex ) * _Color;
				// fixed4 RessourceMap = tex2D(_ResourceMap, IN.uv_ResourceMap) * _Color;
				fixed4 noise = tex2D(_NoiseMap, IN.uv_NoiseMap);
				fixed3 normal = UnpackNormal (tex2D (_BumpMap, IN.uv_MainTex));
				fixed4 Occ = tex2D(_OcclusionMap, IN.uv_OcclusionMap);   


				int i = step(_MetalResourcesInt,0);


				fixed4 t = lerp(mainTex, _ColorResource, noise.r);


				fixed4 endCol = lerp(mainTex, t,i);
				o.Albedo = endCol.rgb * Occ.rgb;
				o.Normal = normal;
				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = endCol.a;
			}
			ENDCG
		}
			FallBack "Diffuse"
}
