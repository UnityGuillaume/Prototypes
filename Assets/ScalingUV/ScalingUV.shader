// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/ScalingUV" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NormalMap("Normal Map", 2D) = "bump" {}
		_Metallic("MetallicMap", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 4.0

		sampler2D _MainTex;
		sampler2D _Metallic;
		sampler2D _NormalMap;
		
		struct Input {
			float2 uv_MainTex;
			float3 localNormal;
			float3 localTangent;
		};

		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.localNormal = normalize(v.normal);
			o.localTangent = normalize(v.tangent);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			float3 scale = float3(length(unity_ObjectToWorld[0]), length(unity_ObjectToWorld[1]), length(unity_ObjectToWorld[2]));

			float3 bitangent = normalize(cross(IN.localTangent, IN.localNormal));

			float3 tangentDot;
			tangentDot.x = dot(IN.localTangent, float3(1, 0, 0));
			tangentDot.y = dot(IN.localTangent, float3(0, 1, 0));
			tangentDot.z = dot(IN.localTangent, float3(0, 0, 1));

			tangentDot = tangentDot * scale;

			float3 bitangentDot;
			bitangentDot.x = dot(bitangent, float3(1, 0, 0));
			bitangentDot.y = dot(bitangent, float3(0, 1, 0));
			bitangentDot.z = dot(bitangent, float3(0, 0, 1));
			
			bitangentDot = bitangentDot * scale;

			float2 uv = IN.uv_MainTex;
			uv.x *= tangentDot.x + tangentDot.y + tangentDot.z;
			uv.y *= bitangentDot.x + bitangentDot.y + bitangentDot.z;

			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, uv) * _Color;
			float4 metalsmooth = tex2D(_Metallic, uv);

			o.Albedo = c.rgb;
			o.Normal = UnpackNormal(tex2D(_NormalMap, uv));
			o.Metallic = metalsmooth.r;
			o.Smoothness = metalsmooth.a;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
