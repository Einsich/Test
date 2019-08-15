﻿Shader "Custom/MapItems" {
	Properties {
	_MainTex("Main Texture", 2D) = "white" {}
	_NormalMap ("Normal Map", 2D) = "white" {}
	}
		SubShader{
			Tags{ "RenderType" = "Opaque" }
			LOD 200
			CGPROGRAM
			#pragma surface surf Standard fullforwardshadows
			#pragma target 3.0
			#include "MapData.cginc"
			sampler2D _MainTex,_NormalMap;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float4 c = tex2D (_MainTex, IN.uv_MainTex);

			c = ShadedColor(IN.worldPos, c);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
		}
		ENDCG
	}
	FallBack "Diffuse"
}
