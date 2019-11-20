//This version of the shader does support shadows, but it does not support transparent outlines

Shader "Custom/CellWithOutline"
{
	Properties
	{
		[Header(Base Parameters)]
		_Color("Tint", Color) = (1, 1, 1, 1)
		_MainTex("Texture", 2D) = "white" {}
		_DissolveTex("Texture", 2D) = "white" {}
		_Specular("Specular Color", Color) = (1,1,1,1)
		[HDR] _Emission("Emission", color) = (0 ,0 ,0 , 1)

		[Header(Lighting Parameters)]
		_ShadowTint("Shadow Color", Color) = (0.5, 0.5, 0.5, 1)
		[IntRange]_StepAmount("Shadow Steps", Range(1, 16)) = 2
		_StepWidth("Step Size", Range(0, 1)) = 0.25
		_SpecularSize("Specular Size", Range(0, 1)) = 0.1
		_SpecularFalloff("Specular Falloff", Range(0, 2)) = 1
		
		_FirstOutlineColor("Outline color", Color) = (1,0,0,0.5)
		_FirstOutlineWidth("Outlines width", Range(0.0, 2.0)) = 0.15

		_Angle("Switch shader on angle", Range(0.0, 180.0)) = 89
	}

		CGINCLUDE
		#include "UnityCG.cginc"

		struct appdata 
		{
			float4 vertex : POSITION;
			float4 normal : NORMAL;
		};

		uniform float4 _FirstOutlineColor;
		uniform float _FirstOutlineWidth;

		uniform float _Angle;

		ENDCG

		SubShader
		{
			//First outline
			Pass
			{
				Tags{ "Queue" = "Geometry" }
				Cull Front
				CGPROGRAM

				struct v2f 
				{
					float4 pos : SV_POSITION;
				};

				#pragma vertex vert
				#pragma fragment frag

				v2f vert(appdata v) 
				{
					appdata original = v;

					float3 scaleDir = normalize(v.vertex.xyz - float4(0,0,0,1));
					//This shader consists of 2 ways of generating outline that are dynamically switched based on demiliter angle
					//If vertex normal is pointed away from object origin then custom outline generation is used (based on scaling along the origin-vertex vector)
					//Otherwise the old-school normal vector scaling is used
					//This way prevents weird artifacts from being created when using either of the methods
					if (degrees(acos(dot(scaleDir.xyz, v.normal.xyz))) > _Angle)
					{
						v.vertex.xyz += normalize(v.normal.xyz) * _FirstOutlineWidth;
					}
					else
					{
						v.vertex.xyz += scaleDir * _FirstOutlineWidth;
					}

					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					return o;
				}

				half4 frag(v2f i) : COLOR
				{
					float4 color = _FirstOutlineColor;
					color.a = 1;
					return color;
				}

				ENDCG
			}

					//the material is completely non-transparent and is rendered at the same time as the other opaque geometry
					Tags{ "RenderType" = "Opaque" "Queue" = "Geometry"}

					CGPROGRAM

					//the shader is a surface shader, meaning that it will be extended by unity in the background to have fancy lighting and other features
					//our surface shader function is called surf and we use our custom lighting model
					//fullforwardshadows makes sure unity adds the shadow passes the shader might need
					#pragma surface surf Stepped fullforwardshadows
					#pragma target 3.0

					sampler2D _MainTex;
					sampler2D _DissolveTex;
					fixed4 _Color;
					half3 _Emission;
					fixed4 _Specular;

					float3 _ShadowTint;
					float _StepWidth;
					float _StepAmount;
					float _SpecularSize;
					float _SpecularFalloff;

					struct ToonSurfaceOutput
					{
						fixed3 Albedo;
						half3 Emission;
						fixed3 Specular;
						fixed Alpha;
						fixed3 Normal;
					};

					//our lighting function. Will be called once per light
					float4 LightingStepped(ToonSurfaceOutput s, float3 lightDir, half3 viewDir, float shadowAttenuation)
					{
						//how much does the normal point towards the light?
						float towardsLight = dot(s.Normal, lightDir);

						//stretch values so each whole value is one step
						towardsLight = towardsLight / _StepWidth;
						//make steps harder
						float lightIntensity = floor(towardsLight);

						// calculate smoothing in first pixels of the steps and add smoothing to step, raising it by one step
						// (that's fine because we used floor previously and we want everything to be the value above the floor value, 
						// for example 0 to 1 should be 1, 1 to 2 should be 2 etc...)
						float change = fwidth(towardsLight);
						float smoothing = smoothstep(0, change, frac(towardsLight));
						lightIntensity = lightIntensity + smoothing;

						// bring the light intensity back into a range where we can use it for color
						// and clamp it so it doesn't do weird stuff below 0 / above one
						lightIntensity = lightIntensity / _StepAmount;
						lightIntensity = saturate(lightIntensity);

					#ifdef USING_DIRECTIONAL_LIGHT
						//for directional lights, get a hard vut in the middle of the shadow attenuation
						float attenuationChange = fwidth(shadowAttenuation) * 0.5;
						float shadow = smoothstep(0.5 - attenuationChange, 0.5 + attenuationChange, shadowAttenuation);
					#else
						//for other light types (point, spot), put the cutoff near black, so the falloff doesn't affect the range
						float attenuationChange = fwidth(shadowAttenuation);
						float shadow = smoothstep(0, attenuationChange, shadowAttenuation);
					#endif
						lightIntensity = lightIntensity * shadow;

						//calculate how much the surface points points towards the reflection direction
						float3 reflectionDirection = reflect(lightDir, s.Normal);
						float towardsReflection = dot(viewDir, -reflectionDirection);

						//make specular highlight all off towards outside of model
						float specularFalloff = dot(viewDir, s.Normal);
						specularFalloff = pow(specularFalloff, _SpecularFalloff);
						towardsReflection = towardsReflection * specularFalloff;

						//make specular intensity with a hard corner
						float specularChange = fwidth(towardsReflection);
						float specularIntensity = smoothstep(1 - _SpecularSize, 1 - _SpecularSize + specularChange, towardsReflection);
						//factor inshadows
						specularIntensity = specularIntensity * shadow;

						float4 color;
						//calculate final color
						color.rgb = s.Albedo * lightIntensity * _LightColor0.rgb;
						color.rgb = lerp(color.rgb, s.Specular * _LightColor0.rgb, saturate(specularIntensity));

						color.a = s.Alpha;
						return color;
					}


					//input struct which is automatically filled by unity
					struct Input
					{
						float2 uv_MainTex;
					};

					//the surface shader function which sets parameters the lighting function then uses
					void surf(Input i, inout ToonSurfaceOutput o)
					{
						//sample and tint albedo texture
						fixed4 col = tex2D(_MainTex, i.uv_MainTex);
						fixed4 dis = tex2D(_DissolveTex, i.uv_MainTex);

						//Convert dissolve progression to -1 to 1 scale.
						//half dBase = -2.0f * _DissolveScale + 1.0f;
						//half dTextRead = dis.r + dBase;

						col *= _Color;
						o.Albedo = col.rgb;

						o.Specular = _Specular;

						float3 shadowColor = col.rgb * _ShadowTint;
						o.Emission = _Emission + shadowColor;

						//half alpha = clamp(dTexRead, 0.0f, 1.0f);

					}
					ENDCG

				}
					FallBack "Standard"
		}