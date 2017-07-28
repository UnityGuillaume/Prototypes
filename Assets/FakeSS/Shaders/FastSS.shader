Shader "Custom/FastSS" {
	Properties {
		_MainTint("Color", Color) = (1,1,0,1)

		_Albedo("Albedo", 2D) = "white" {}
		_NormalMap("Normal Map", 2D) = "bump" {}
		_MetalSmoothness("Metal Smoothness", 2D) = "white" {}

		_ThicknessMap("Thickness", 2D) = "white" {}

		_Distortion("Distortion", Float) = 1.0
		_LTPower("LTPower", Float) = 1.0
		_LTScale("LTScale", Float) = 1.0
		_LTAmbient("Ambient", Float) = 1.0
	}
		SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surfcust FastSS

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 4.0 

		#include "UnityPBSLighting.cginc"

		float _Distortion;
		float _LTPower;
		float _LTScale;
		float _LTAmbient;

		inline void LightingFastSS_GI(SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi)
		{
#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
			gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
#else
			Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(0.0f, data.worldViewDir, s.Normal, lerp(unity_ColorSpaceDielectricSpec.rgb, s.Albedo, 0.0f));
			gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
#endif
		}

		inline half4 LightingFastSS_Deferred(SurfaceOutputStandard s, half3 viewDir, UnityGI gi, out half4 outDiffuseOcclusion, out half4 outSpecSmoothness, out half4 outNormal)
		{
			half oneMinusReflectivity;
			half3 specColor;
			s.Albedo = DiffuseAndSpecularFromMetallic(s.Albedo, s.Metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

			half4 c = UNITY_BRDF_PBS(s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);

			UnityStandardData data;
			data.diffuseColor = s.Albedo;
			data.occlusion = s.Occlusion;
			data.smoothness = s.Smoothness;

			//we replace specular color with float3(_Distortion, _LTPower, _LTAmbient)
			data.specularColor = float3(_Distortion, s.Alpha, _LTAmbient);
			
			data.normalWorld = s.Normal;

			UnityStandardDataToGbuffer(data, outDiffuseOcclusion, outSpecSmoothness, outNormal);

			outNormal.a = 0.5;

			half4 emission = half4(s.Emission + c.rgb, 1);

			return emission;
		}

		sampler2D _Albedo;
		sampler2D _NormalMap;
		sampler2D _MetalSmoothness;
		sampler2D _ThicknessMap;

		struct Input {
			float2 uv_ThicknessMap;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _MainTint;

		void surfcust (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_Albedo, IN.uv_ThicknessMap);
			fixed4 ms = tex2D(_MetalSmoothness, IN.uv_ThicknessMap);

			o.Alpha = 1.0f - tex2D(_ThicknessMap, IN.uv_ThicknessMap).r;
			o.Albedo = _MainTint * c;
			// Metallic and smoothness come from slider variables
			o.Metallic = ms.rgb;
			o.Smoothness = ms.a;

			o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_ThicknessMap));
		}
		ENDCG
	}
	FallBack "Diffuse"
}
