// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/Terrain/Standard" 
{
    Properties 
    {
        // used in fallback on old cards & base map
        [HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
        [HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
        _ColorResource("ColorResource", Color) = (1,1,1,1)
        _NoiseMap("Noise (RGB)", 2D) = "white" {}
        _TerritorriumMap("Territorium (RGB)", 2D) = "white" {}

        _MapChange("MapChange", Int) = 0.0

    }

    SubShader 
    {
        Tags 
        {
            "Queue" = "Geometry-100"
            "RenderType" = "Opaque"
        }

        CGPROGRAM
        #pragma surface surf Standard vertex:SplatmapVert finalcolor:SplatmapFinalColor finalgbuffer:SplatmapFinalGBuffer addshadow fullforwardshadows
        #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
        #pragma multi_compile_fog // needed because finalcolor oppresses fog code generation.
        #pragma target 3.0
        // needs more than 8 texcoords
        #pragma exclude_renderers gles
        #include "UnityPBSLighting.cginc"

        #pragma multi_compile_local __ _NORMALMAP

        #define TERRAIN_STANDARD_SHADER
        #define TERRAIN_INSTANCED_PERPIXEL_NORMAL
        #define TERRAIN_SURFACE_OUTPUT SurfaceOutputStandard
        #include "TerrainSplatmapCommon.cginc"

	
        fixed4 _ColorResource;
		int _MetalResourcesInt;
		int _TerrriitorumMapInt;
        int _MapChange;

		sampler2D _NoiseMap;
		sampler2D _TerritorriumMap;


        half _Metallic0;
        half _Metallic1;
        half _Metallic2;
        half _Metallic3;

        half _Smoothness0;
        half _Smoothness1;
        half _Smoothness2;
        half _Smoothness3;

        void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			fixed4 noiseMap = tex2D(_NoiseMap, IN.tc.xy);
			fixed4 territoriumMap = tex2D(_TerritorriumMap, IN.tc.xy);

            half4 splat_control;
            half weight;
            fixed4 mixedDiffuse;
            half4 defaultSmoothness = half4(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3);
            SplatmapMix(IN, defaultSmoothness, splat_control, weight, mixedDiffuse, o.Normal);

			int resourceInt = step(1.0 , _MapChange);
            int TerritourriumInt = step(2.0 , _MapChange);

			fixed4 t = lerp(mixedDiffuse.rgba, _ColorResource, noiseMap.r);
            fixed4 finalMap = lerp(t, territoriumMap, TerritourriumInt);

			fixed4 endCol = lerp(mixedDiffuse.rgba, finalMap, resourceInt);

            o.Albedo = endCol.rgb;
            o.Alpha = weight;
            o.Smoothness = mixedDiffuse.a;
            o.Metallic = dot(splat_control, half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3));
        }
        ENDCG

        UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
        UsePass "Hidden/Nature/Terrain/Utilities/SELECTION"
    }

    Dependency "AddPassShader"    = "Hidden/TerrainEngine/Splatmap/Standard-AddPass"
    Dependency "BaseMapShader"    = "Hidden/TerrainEngine/Splatmap/Standard-Base"
    Dependency "BaseMapGenShader" = "Hidden/TerrainEngine/Splatmap/Standard-BaseGen"

    Fallback "Nature/Terrain/Diffuse"
}
