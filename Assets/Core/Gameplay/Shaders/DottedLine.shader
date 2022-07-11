Shader "Custom/DottedLine"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _LentUVYPose("Lent UV y pose", float) = 0.0
        _CutOutTreshold("Cut Out Treshold",Range(0,1))=0.5
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" "IgnoreProjector"="True" }        
        AlphaToMask On
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half _LentUVYPose;
            float4 _Color;
            float _CutOutTreshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            float2 Flow(float2 uv, float time) {
                float2 dir;
                dir.x = 0.0;
                dir.y = -_LentUVYPose * time * 0.05;
                return uv + dir;
		    }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = Flow(i.uv, _Time.y);
                fixed4 col = tex2D(_MainTex, uv);
                clip(col.a-_CutOutTreshold);
                return col * _Color;
            }
            ENDCG
        }
    }
}
