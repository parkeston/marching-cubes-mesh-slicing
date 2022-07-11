// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Slice" {
Properties {
	_Volume ("Texture", 3D) = "white" {}
	_Depth("Depth", Range(0,1)) = 0.5
    _CutOutTreshold("Cut Out Treshold",Range(-1,1)) = 0
	
	[Space]
	_MinColor("Min Color",Color) = (0,0,0,1)
	_MaxColor("Max Color",Color) = (1,1,1,1)
	_ColorSpeed("Color Speed",float) = 1
}
SubShader {
	
	Tags{"Queue" = "Transparent"}
	Blend SrcAlpha OneMinusSrcAlpha
	
	Pass {

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		 
		#include "UnityCG.cginc"
		 
		struct vs_input {
		    float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};
		 
		struct ps_input {
		    float4 pos : SV_POSITION;
		    float2 uv : TEXCOORD0;
		};
		 
		 
		ps_input vert (vs_input v)
		{
		    ps_input o;
		    o.pos = UnityObjectToClipPos (v.vertex);
		    o.uv = v.uv;
		    return o;
		}
		 
		sampler3D _Volume;		
		float _Depth;
		float _CutOutTreshold;

		float4 _MinColor;
		float4 _MaxColor;
		float _ColorSpeed;

		float4 frag (ps_input i) : COLOR
		{
			float value = tex3D(_Volume,float3(i.uv.xy,_Depth)).r;
			clip(value+_CutOutTreshold);

			float t = sin(_Time.y*_ColorSpeed)/2.0f+0.5f;
			return lerp(_MinColor,_MaxColor,t);
		}
		 
		ENDCG
		 
		}
	}
	 
	Fallback "VertexLit"
}
