
Shader "MyShaders/TwoColorsShadow" {
	Properties 
	{
		_TopColor ("Top Color", Color) = (.5,.5,.5,1)
		_SideColor ("Side Color", Color) = (.5,.5,.5,1)
		_ShadowColor ("Shadow Color", Color) = (0,0,0,1)
	}

	SubShader {
		Tags {"Queue"="Geometry+2" "RenderType"="Opaque" "LightMode" = "ForwardBase" }
		Pass 
		{
			Name "BASE"
			Cull back
			//ZWrite off
			Lighting Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"

			#pragma multi_compile_fwdbase
			
			float4 _TopColor;
			float4 _SideColor;	
            float4 _ShadowColor;
            
			struct appdata 
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
			
			struct v2f 
			{
				float4 pos : SV_POSITION;
				float normalY : float;
				SHADOW_COORDS(1)
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.normalY = UnityObjectToWorldNormal(v.normal).y;
				TRANSFER_SHADOW(o);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
			    float atten = SHADOW_ATTENUATION(i);
			    fixed4 color = lerp(_SideColor,_TopColor,i.normalY );// * atten;
                color.rgb = color.rgb * atten + _ShadowColor * (1-atten);
                return color;
			} 
			ENDCG			
		}
		
		// shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	} 

	Fallback "VertexLit"
}
