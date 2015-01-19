Shader "VertexLit CG" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Spec Color", Color) = (1,1,1,0)
	_Emission ("Emissive Color", Color) = (0,0,0,0)
	_Shininess ("Shininess", Range (0.1, 1)) = 0.7
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_Attenuation ("Attenuation", int) = 6
}
 
SubShader {
	Tags {"Queue"="Geometry"  "IgnoreProjector"="True"}
	LOD 100
 
	ZWrite On
 
	Pass {
		Tags { LightMode = Vertex } 
		CGPROGRAM
		#pragma vertex vert  
		#pragma fragment frag
 
		#include "UnityCG.cginc"
 
		#define ADD_SPECULAR
 
		fixed4 _Color;
		fixed4 _SpecColor;
		fixed4 _Emission;
 
		half _Shininess;
		
		int _Attenuation;
 
		sampler2D _MainTex;
		float4 _MainTex_ST;
 
		struct v2f {
			float4 pos : SV_POSITION;
			float2 uv_MainTex : TEXCOORD0;
			fixed3 diff : COLOR;
 
			#ifdef ADD_SPECULAR
			fixed3 spec : TEXCOORD1;
			#endif
		};
 
		v2f vert (appdata_full v)
		{
		    v2f o;
		    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
		    o.uv_MainTex = TRANSFORM_TEX (v.texcoord, _MainTex);
 
			float3 viewpos = mul (UNITY_MATRIX_MV, v.vertex).xyz;
 
			o.diff = UNITY_LIGHTMODEL_AMBIENT.xyz;
 
			#ifdef ADD_SPECULAR
			o.spec = 0;
			fixed3 viewDirObj = normalize( ObjSpaceViewDir(v.vertex) );
			#endif
 
			//All calculations are in object space
			for (int i = 0; i < 4; i++) {
				half3 toLight = unity_LightPosition[i].xyz - viewpos.xyz * unity_LightPosition[i].w;
				half lengthSq = dot(toLight, toLight);
				half atten = pow(150.0, _Attenuation) / (1.0 + pow(lengthSq, _Attenuation) * unity_LightAtten[i].z);
 
				fixed3 lightDirObj = mul( (float3x3)UNITY_MATRIX_T_MV, toLight);	//View => model
 
				lightDirObj = normalize(lightDirObj);
 
				fixed diff = max ( 0, dot (v.normal, lightDirObj) );
				o.diff += unity_LightColor[i].rgb * (diff * atten);
 
				#ifdef ADD_SPECULAR
				fixed3 h = normalize (viewDirObj + lightDirObj);
				fixed nh = max (0, dot (v.normal, h));
 
				fixed spec = pow (nh, _Shininess * 128.0) * 0.5;
				o.spec += spec * unity_LightColor[i].rgb * atten;
				#endif
			}
 
			o.diff = (o.diff * _Color + _Emission.rgb) * 2;
			#ifdef ADD_SPECULAR
			o.spec *= 2 * _SpecColor;
			#endif
 
			return o;
		}
 
		fixed4 frag (v2f i) : COLOR {
			fixed4 c;
 
			fixed4 mainTex = tex2D (_MainTex, i.uv_MainTex);
 
			#ifdef ADD_SPECULAR
			c.rgb = (mainTex.rgb * i.diff + i.spec);
			#else
			c.rgb = (mainTex.rgb * i.diff);
			#endif
 
			c.a = 1;
 
			return c;
		}
 
		ENDCG
	}
 
	//Lightmap pass, dLDR;
	Pass {
		Tags { "LightMode" = "VertexLM" }
 
		CGPROGRAM
		#pragma vertex vert  
		#pragma fragment frag
 
		#include "UnityCG.cginc"
 
		float4 unity_LightmapST;
		sampler2D unity_Lightmap;
 
		struct v2f {
			float4 pos : SV_POSITION;
			float2 lmap : TEXCOORD0;
		};
 
		v2f vert (appdata_full v)
		{
		    v2f o;
		    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
 
		    o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
 
		    return o;
		 }
 
		fixed4 frag (v2f i) : COLOR {
			fixed4 lmtex = tex2D(unity_Lightmap, i.lmap.xy);
			fixed3 lm = (8.0 * lmtex.a) * lmtex.rgb;
			return fixed4(lm, 1);
		}
 
		ENDCG
	}
 
	//Lightmap pass, RGBM;
	Pass {
		Tags { "LightMode" = "VertexLMRGBM" }
 
		CGPROGRAM
		#pragma vertex vert  
		#pragma fragment frag
 
		#include "UnityCG.cginc"
 
		float4 unity_LightmapST;
		sampler2D unity_Lightmap;
 
		struct v2f {
			float4 pos : SV_POSITION;
			float2 lmap : TEXCOORD0;
		};
 
		v2f vert (appdata_full v)
		{
		    v2f o;
		    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
 
		    o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
 
		    return o;
		 }
 
		fixed4 frag (v2f i) : COLOR {
			fixed4 lmtex = tex2D(unity_Lightmap, i.lmap.xy);
			fixed3 lm = (8.0 * lmtex.a) * lmtex.rgb;
			return fixed4(lm, 1);
		}
 
		ENDCG
	}
}
 
Fallback "VertexLit"
}