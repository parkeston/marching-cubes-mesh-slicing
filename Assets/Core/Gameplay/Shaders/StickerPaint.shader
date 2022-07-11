Shader "Custom/StickerPaint" 
{
	Properties 
	{
		_DecorCutOutTreshold("Decor CutoutTreshold",Range(0,1))=0
		_StickerColor("StickerColor",Color) = (1,1,1,1)
		_StickerBackSideColor("Sticker Back Side Color", Color) = (1,1,1,1)
	}
	
	SubShader 
	{
		Tags {"Queue" = "Transparent"}
		
		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			 
			#include "UnityCG.cginc"
			#include "UvRemap.cginc"
			 
			struct vs_input
			{
			    float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			 
			struct ps_input
			{
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

			uniform sampler2D _ShapeTexture;
			uniform sampler2D _SprayTexture;
			
			float _DecorCutOutTreshold;
			float4 _StickerColor;

			float4 frag (ps_input i) : COLOR
			{
				clip(tex2D(_ShapeTexture, i.uv).a);
				clip(_DecorCutOutTreshold- GetRemappedUVColor(i.uv));

				//"alpha" blending with opaque background and brush (fix hard edges through time)
				float4 sprayColor = tex2D(_SprayTexture,i.uv);
				float3 result = sprayColor.rgb+_StickerColor.rgb*(1-sprayColor.a);
				return float4(result,1);
			}
			 
			ENDCG
			 
		}
	}
	 
	Fallback "VertexLit"
}
