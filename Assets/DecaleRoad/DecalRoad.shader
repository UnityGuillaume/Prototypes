Shader "Custom/DecalRoad" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows
		#include "UnityGBuffer.cginc"

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _CameraGBufferTexture0;
		sampler2D _CameraGBufferTexture1;
		sampler2D _CameraGBufferTexture2;

		struct Input {
			float2 uv_MainTex;
			float4 screenPos;
		}; 

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			half2 screenUV = IN.screenPos.xy / IN.screenPos.w;
			half4 gbuffer0 = tex2D(_CameraGBufferTexture0, screenUV);
			half4 gbuffer1 = tex2D(_CameraGBufferTexture1, screenUV);
			half4 gbuffer2 = tex2D(_CameraGBufferTexture2, screenUV);

			UnityStandardData data = UnityStandardDataFromGbuffer(gbuffer0, gbuffer1, gbuffer2);

			o.Albedo = data.diffuseColor;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = data.smoothness;
			o.Alpha = 1.0f;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
