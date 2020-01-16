Shader "Custom/BuildingShader"
{
	Properties
	{
		[Header(Base Parameters)]
		_Color("Tint", Color) = (1, 1, 1, 1)
		_InsideColor("InsideColor", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_BumpMap("Bumpmap", 2D) = "bump" {}
		_BumpMapValue("BumpmapValue", Float) = 1.0
		_ParallaxMap("Heightmap", 2D) = "black" {}
		_Parallax("Height", Range(0.005, 0.08)) = 0.02
		_AmbientMap("AO_Map (R)", 2D) = "black" {}
		_AmbientValue("AO_Value", Float) = 0.0
		_Specular("Specular", 2D) = "black" {}
		//	_Specular("Specular Color", Color) = (1,1,1,1)

		_Glossiness("Smoothness", Range(0,1)) = 0.5
		//_Metallic("Metallic", Range(0,1)) = 0.0

		_EmissionMap("EmissionMap", 2D) = "black" {}
		[PerRendererData][HDR] _Emission("Emission", color) = (0 ,0 ,0 , 1)

		[Header(Noise Parameters)]
		_Amplitude("Amplitude", Vector) = (0,0,0,0)
		_Frequency("Frequency", Vector) = (0,0,0,0)
		_Phase("Phase", Vector) = (0,0,0,0)

		[PerRendererData]_Clip("Clip Height", Float) = 0.0
			//_Clip("Clip Height", Float) = 0.0
			_Noise("Noise", Float) = 0.0
			_NoiseScale("Noise Scale", Float) = 1.0
			_BuildingHeight("BuildingHeight", Float) = 5.0

	}

		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			CGPROGRAM
			//#pragma surface surf Lambert vertex:vert
			#pragma surface surf StandardSpecular	 vertex:vert
			
			#pragma target 3.0
			#pragma shader_feature _USEMETALLICMAP_ON
	//		#include "UnityCG.cginc"
			#include "UnityStandardUtils.cginc" 
			#include "Includes/SimplexNoise3D.cginc"


			struct Input
			{
				float3 viewDir;
				float3 worldPos;
				float2 uv_MainTex;
				float2 uv_BumpMap;
				float2 uv_ParallaxMap;
				float3 localPos;
				half frontFace : VFACE;
			};

			  sampler2D _MainTex;
			  sampler2D _EmissionMap;
			  sampler2D _BumpMap;
			  sampler2D _Specular;
			  sampler2D _ParallaxMap;
			  sampler2D _AmbientMap;

			  fixed4 _Color;
			  float _BumpMapValue;
			  float3 _ShadowTint;

			  half3 _Emission;
			  half _Glossiness;
			  half _Metallic;
			  half _AmbientValue;
			  //float _StepWidth;
			  //float _StepAmount;
			  //float _SpecularSize;
			  //float _SpecularFalloff;

				// Noise
			  fixed4 _InsideColor;
			  float3 _Amplitude;
			  float3 _Frequency;
			  float3 _Phase;
			  float _Clip;
			  float _Noise;
			  float _NoiseScale;
			  float _BuildingHeight;
			  float _Parallax;




			  void vert(inout appdata_full v, out Input o)
			  {
				  UNITY_INITIALIZE_OUTPUT(Input, o);
				  o.localPos = v.vertex.xyz;
			  }

			  void surf(Input i, inout SurfaceOutputStandardSpecular o)
			  {
				  half h = tex2D(_ParallaxMap, i.uv_ParallaxMap).w;
				  float2 offset = ParallaxOffset(h, _Parallax, i.viewDir);

				  i.uv_MainTex += offset;
				  i.uv_BumpMap += offset;

				  float3 p = i.localPos * 100;
				  //float3 p = i.localPos;
				  float3 s = _NoiseScale * p;
				  float noise = snoise(s);
				  noise *= _Noise;
				  float3 n = sin((noise + p.xyz) * _Frequency.xyz + _Phase.xyz + noise) * _Amplitude.xyz;
				  float d = p.z + length(n);
				  noise = d;

				  float tmp = _Clip * _BuildingHeight;

				  if (step(d, tmp) <= 0.0)
				  {
					  discard;
				  }

				  float4 color = lerp(_InsideColor, _Color, i.frontFace);
				 
				  //sample and tint albedo texture
				  fixed4 col = tex2D(_MainTex, i.uv_MainTex);

				  col *= _Color;
				  o.Albedo = col.rgb;
				  o.Normal = UnpackScaleNormal(tex2D(_BumpMap, i.uv_BumpMap), _BumpMapValue);

				  half AOTex = tex2D(_AmbientMap, i.uv_MainTex).r;
				  o.Occlusion = AOTex * _AmbientValue;

				  //o.Metallic = _Metallic;
				  
				  o.Specular = tex2D(_Specular, i.uv_MainTex).a;
				  o.Smoothness = _Glossiness;

				  float4 emi = tex2D(_EmissionMap, i.uv_MainTex);
				  o.Emission = emi * _Emission;
			  }
			  ENDCG
		}
			Fallback "Diffuse"
}