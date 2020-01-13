Shader "Custom/Soldier_Shader"
{
	Properties
	{
		[Header(Base Parameters)]
		_Color("Tint", Color) = (1, 1, 1, 1)
		_InsideColor("InsideColor", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_BumpMap("Bumpmap", 2D) = "bump" {}
		_BumpMapValue("BumpmapValue", Float) = 1.0
		_ParallaxMap("Heightmap (A)", 2D) = "black" {}
		_Parallax("Height", Range(0.005, 0.08)) = 0.02
		_Specular("Specular", 2D) = "black" {}
		//	_Specular("Specular Color", Color) = (1,1,1,1)
		_EmissionMap("EmissionMap", 2D) = "white" {}
		[PerRendererData][HDR] _Emission("Emission", color) = (0 ,0 ,0 , 1)
		//[HDR] _Emission("Emission", color) = (0 ,0 ,0 , 1)

		[Header(Noise Parameters)]
		_Amplitude("Amplitude", Vector) = (0,0,0,0)
		_Frequency("Frequency", Vector) = (0,0,0,0)
		_Phase("Phase", Vector) = (0,0,0,0)

		_Clip("Clip Height", Float) = 0.0
		_Noise("Noise", Float) = 0.0
		_NoiseScale("Noise Scale", Float) = 1.0
		_BuildingHeight("BuildingHeight", Float) = 5.0

	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		CGPROGRAM
		#pragma surface surf Lambert
		#pragma target 3.0
		#include "UnityStandardUtils.cginc" 
		#include "Includes/SimplexNoise3D.cginc"


		struct Input
		{
			float3 viewDir;
			float3 worldPos;
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_ParallaxMap;
			half frontFace : VFACE;
		};

		  sampler2D _MainTex;
		  sampler2D _EmissionMap;
		  sampler2D _BumpMap;
		  sampler2D _Specular;
		  sampler2D _ParallaxMap;			

		  fixed4 _Color;
		  half3 _Emission;
		  float _BumpMapValue;

		  float3 _ShadowTint;
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


		  void surf(Input i, inout SurfaceOutput o)
		  {
			  //TO-DO: DISSOLVE RAUSWERFEN
			  half h = tex2D(_ParallaxMap, i.uv_ParallaxMap).w;
			  float2 offset = ParallaxOffset(h, _Parallax, i.viewDir);

			  i.uv_MainTex += offset;
			  i.uv_BumpMap += offset;

			  float3 p = i.worldPos;
			  float3 s = _NoiseScale * p;
			  float noise = snoise(s);
			  noise *= _Noise;
			  float3 n = sin((noise + p.xyz) * _Frequency.xyz + _Phase.xyz + noise) * _Amplitude.xyz;
			  float d = p.y + length(n);
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
			  o.Albedo = col.rgb /** color*/;
			  //o.Normal = UnpackNormal(tex2D(_BumpMap, i.uv_BumpMap)) /** _BumpMapValue*/;
			  o.Normal = UnpackScaleNormal(tex2D(_BumpMap, i.uv_BumpMap), _BumpMapValue);

			  o.Specular = tex2D(_Specular, i.uv_MainTex);
			  //o.Emission = _Emission + shadowColor;
			  //half alpha = clamp(dTexRead, 0.0f, 1.0f);

			  half rim = 1.0 - saturate(dot(normalize(i.viewDir), o.Normal));

			  float4 emi = tex2D(_EmissionMap, i.uv_MainTex);
			  o.Emission = emi * _Emission;
		  }
		  ENDCG
	  }
		  Fallback "Diffuse"
}