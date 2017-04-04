Shader "Custom/KitUberShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NormalMap("Normal Map", 2D) = "normal" {}
		_HeightMap("Height Map", 2D) = "white" {}

		_HeightMapScale("HeightMap Scale", Range(0.0, 5.0)) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Cull Off

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 4.0

		sampler2D _MainTex;
		sampler2D _NormalMap;
		sampler2D _HeightMap;

		struct Input {
			float2 uv_MainTex;
			float3 viewDir;
			float3 tangentEye;
			float3 worldNormal; INTERNAL_DATA
		};

		fixed4 _Color;
		float _HeightMapScale;

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			TANGENT_SPACE_ROTATION;
			o.tangentEye = mul(rotation, mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos);
		}

		const int nMinSamples = 8;
		const int nMaxSamples = 20;

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			float parallaxLimit = -length(IN.tangentEye.xy) / IN.tangentEye.z;
			parallaxLimit *= _HeightMapScale;

			float2 vOffsetDir = normalize(IN.tangentEye.xy);
			float2 vMaxOffset = vOffsetDir * parallaxLimit;

			int nNumSamples = 10;// (int)lerp(nMaxSamples, nMinSamples, dot(IN.viewDir, IN.worldNormal));
			float fStepSize = 1.0 / (float)nNumSamples;

			float2 dx = ddx(IN.uv_MainTex);
			float2 dy = ddy(IN.uv_MainTex);

			float fCurrRayHeight = 1.0;
			float2 vCurrOffset = float2(0, 0);
			float2 vLastOffset = float2(0, 0);

			float fLastSampledHeight = 1;
			float fCurrSampledHeight = 1;

			int nCurrSample = 0;

			float4 col = float4(1,0,0,1);
			while (nCurrSample < nNumSamples)
			{
				fCurrSampledHeight = tex2D(_HeightMap, IN.uv_MainTex).r;
				if (fCurrSampledHeight > fCurrRayHeight)
				{
					float delta1 = fCurrSampledHeight - fCurrRayHeight;
					float delta2 = (fCurrRayHeight + fStepSize) - fLastSampledHeight;

					float ratio = delta1 / (delta1 + delta2);

					vCurrOffset = (ratio)* vLastOffset + (1.0 - ratio) * vCurrOffset;

					nCurrSample = nNumSamples + 1;
				}
				else
				{
					nCurrSample++;

					fCurrRayHeight -= fStepSize;

					vLastOffset = vCurrOffset;
					vCurrOffset += fStepSize * vMaxOffset;

					fLastSampledHeight = fCurrSampledHeight;
				}
			}

			float2 vFinalCoords = IN.uv_MainTex + vCurrOffset;

			float4 vFinalNormal = tex2D(_NormalMap, vFinalCoords); //.a;
			float4 vFinalColor = tex2D(_MainTex, vFinalCoords);

			// Albedo comes from a texture tinted by color
			fixed4 c = _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			o.Normal = UnpackNormal(vFinalNormal);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
