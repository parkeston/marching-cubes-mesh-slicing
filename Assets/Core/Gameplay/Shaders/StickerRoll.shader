﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/StickerRoll" 
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
		GrabPass{"_Background2Texture"}
		
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
			    float4 screenPos : TEXCOORD1;
			};
			 
			ps_input vert (vs_input v)
			{
			    ps_input o;
			    o.pos = UnityObjectToClipPos (v.vertex);
			    o.uv = v.uv;
				o.screenPos = ComputeGrabScreenPos(o.pos);
			    return o;
			}

			uniform sampler2D _ShapeTexture;
			uniform sampler2D _SprayTexture;
			sampler2D _Background2Texture;
			
			float _DecorCutOutTreshold;
			
			float2 _StickerPointer;
			float2 _StickerEnterPoint;

			float4 _StickerColor;
			float4 _StickerBackSideColor;

			float4 frag (ps_input i) : COLOR
			{
				float radius = 0.1f;
				
				float4 sprayColor = tex2D(_SprayTexture,i.uv);
				float3 result = sprayColor.rgb+_StickerColor.rgb*(1-sprayColor.a);
				float4 originColor =  float4(result, tex2D(_ShapeTexture, i.uv).a);
				float decorColor = GetRemappedUVColor(i.uv);
				
				float4 fragColor;
			    float2 uv =   i.uv;

			    float2 mouse =_StickerPointer;
			    float2 mouseDir = normalize(abs(_StickerEnterPoint)-_StickerPointer);
			    float2 origin = clamp(mouse - mouseDir * (mouse.y / mouseDir.y), 0., 1.);
				
			    float mouseDist = clamp(length(mouse - origin)
		    		+ (1 - _StickerEnterPoint.y * 1) / mouseDir.y, 0., 1 / mouseDir.y);
	    
			    if (mouseDir.y < 0.)
			        mouseDist = distance(mouse, origin);
			    
			    float proj = dot(uv - origin, mouseDir);
			    float dist = proj - mouseDist;
	    
				float2 linePoint = uv - dist * mouseDir;
				
			    if (dist > radius) 
			    {
		    		float2 screenUV = i.screenPos.xy/i.screenPos.w;
		    		fragColor=tex2D(_Background2Texture,screenUV);
				    fragColor.rgb *= pow(clamp((dist - radius)/radius, 0., 1.) * 1.5, .2);
		    		
		    		clip(originColor.a);
			    }
			    else if (dist >= 0.)
			    {
			        // map to cylinder point
			        float theta = asin(dist / radius);
			        float2 p2 = linePoint + mouseDir * (UNITY_PI - theta) * radius;
			        float2 p1 = linePoint + mouseDir * theta * radius;
			        uv = (p2.x <= 1 && p2.y <= 1. && p2.x > 0. && p2.y > 0.) ? p2 : p1;
			        fragColor = tex2D(_ShapeTexture, uv);
    				if(fragColor.r>=0)
    				{
    					fragColor=_StickerBackSideColor;
    					fragColor.rgb *= pow(clamp((radius - dist) / radius, 0., 1.), .2);
    					if((_DecorCutOutTreshold-GetRemappedUVColor(uv))<=0)
    						fragColor.r=-1;
    				}
		    		
		    		if(fragColor.r<0)
		    		{
		    			//if cylinder roll part is transparent just sample as usual (underneath of roll)
		    			fragColor=float4(originColor.rgb,1);
						clip(originColor.a);
		    			clip(_DecorCutOutTreshold-decorColor);
		    		}
			    }
			    else 
			    {
			        float2 p = linePoint + mouseDir * (abs(dist) + UNITY_PI * radius);
		    		bool isPointOnCylinder = (p.x <= 1 && p.y <= 1. && p.x > 0. && p.y > 0.);
			        uv = isPointOnCylinder ? p : uv;
			        fragColor = tex2D(_ShapeTexture, uv);
    				if(fragColor.r>=0)
    				{
    					fragColor=float4(originColor.rgb,1);;
    					if(isPointOnCylinder)
    					{
    						fragColor=_StickerBackSideColor;
    						fragColor.rgb *= pow(clamp((radius - abs(dist))/radius, 0., 1.), .2);
    						if((_DecorCutOutTreshold-GetRemappedUVColor(uv))<=0)
    							fragColor.r=-1;
    					}
    					else
    						clip(_DecorCutOutTreshold-decorColor);
    				}
    				//if edge rolled part is transparent just sample as usual (underneath of roll)
    				if(fragColor.r<=0)
    				{  
    					fragColor=float4(originColor.rgb,1);
						clip(originColor.a);
    					clip(_DecorCutOutTreshold-decorColor);
    				}
			    }

				return fragColor;
			}
			 
			ENDCG
			 
		}
	}
	 
	Fallback "VertexLit"
}
