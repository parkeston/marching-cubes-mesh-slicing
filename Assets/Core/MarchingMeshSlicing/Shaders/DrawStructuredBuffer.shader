Shader "MarchingCubesGPUProject/DrawStructuredBuffer" 
{
	Properties
		{		
			_Color("Color",Color) = (1,1,1,1)
			_MainTex("Texutre",2D) = "white"
		}
	SubShader 
	{
		
		
		Pass 
		{
		    Tags {"Queue"="Transparent" "RenderType"="Transparent"}
		    ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
			Cull back

			CGPROGRAM
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#pragma require compute
			#pragma vertex vert
			#pragma fragment frag
			
			
			struct Vert
			{
				float4 position;
				float3 normal;
				float2 uv;
				float3 tangent;
			};

			uniform StructuredBuffer<Vert> _Buffer;

			struct v2f 
			{
				float4  pos : SV_POSITION;
				float3 col : Color;
				float2 uv : TEXCOORD0;
			};

			fixed4 _Color;
			sampler2D _MainTex;

			v2f vert(uint id : SV_VertexID)
			{
				Vert vert = _Buffer[id];

				v2f OUT;
				OUT.pos = UnityObjectToClipPos(float4(vert.position.xyz, 1));
				half nl = max(0, dot(vert.normal, _WorldSpaceLightPos0.xyz))*0.5f+0.5f;     
				OUT.col = nl * _LightColor0*_Color;
				OUT.col += ShadeSH9(half4(vert.normal,1));
				OUT.uv = vert.uv;
				return OUT;
			}

			float4 frag(v2f IN) : COLOR
			{
				return float4(IN.col*tex2D(_MainTex,IN.uv), _Color.w);
			}

			ENDCG

		}
	}
}