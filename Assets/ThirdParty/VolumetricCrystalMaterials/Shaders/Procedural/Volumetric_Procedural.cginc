/*
Copyright (c) 2017 - Funktronic Labs, Inc. All Rights Reserved

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
POSSIBILITY OF SUCH DAMAGE.
*/

#ifndef __FUNKTRONIC_LABS_VOLUMETRIC_CGINC__
#define __FUNKTRONIC_LABS_VOLUMETRIC_CGINC__

#include "UnityCG.cginc"
#include "Assets/Core/Gameplay/Shaders/UvRemap.cginc"
#include "UnityLightingCommon.cginc" // for _LightColor0

//////////////////////////////////////////////////
// GLOBAL FUNCTIONS
//////////////////////////////////////////////////
float lumin(float3 rgb)
{
	return dot(rgb, float3(0.299, 0.587, 0.114));
}

//////////////////////////////////////////////////
// SHADER UNIFORMS
//////////////////////////////////////////////////
// layer 0
#ifdef EnableLayer0
sampler2D _Layer0Tex;
fixed4 _Layer0Tint;
float4 _Layer0Tex_ST;
float _Layer0SpeedX;
float _Layer0SpeedY;
#endif

// layer 1
#ifdef EnableLayer1
sampler2D _Layer1Tex;
fixed4 _Layer1Tint;
float4 _Layer1Tex_ST;
float _Layer1SpeedX;
float _Layer1SpeedY;
#endif

// layer 2
#ifdef EnableLayer2
sampler2D _Layer2Tex;
fixed4 _Layer2Tint;
float4 _Layer2Tex_ST;
float _Layer2SpeedX;
float _Layer2SpeedY;
#endif

// global layer params
float _LayerDepthFalloff;
float _LayerHeightBias;
float _LayerHeightBiasStep;

// marble tex
sampler2D _MarbleTex;
float4 _MarbleTex_ST;
float _MarbleHeightScale;
float _MarbleHeightCausticOffset;

// caustic
sampler2D _CausticMap;
float4 _CausticMap_ST;
fixed4 _CausticTint;
float _CausticScrollSpeed;

// surface alpha masking
sampler2D _SurfaceAlphaMaskTex;
float4 _SurfaceAlphaMaskTex_ST;
float4 _SurfaceAlphaColor;

// fresnel
#ifdef EnableFresnel
float _FresnelTightness;
float4 _FresnelColorInside;
float4 _FresnelColorOutside;
#endif

// inner light
#ifdef EnableInnerLight
float _InnerLightTightness;
float4 _InnerLightColorInside;
float4 _InnerLightColorOutside;
#endif

// specular
#ifdef EnableSpecular
float _SpecularTightness;
float _SpecularBrightness;
#endif

// refraction
#if defined(EnableRefraction) && !defined(VOLUMETRIC_MOBILE_VER)
float _RefractionStrength;
sampler2D _BackgroundTexture;
#endif

sampler2D _OpacityTexture;

#ifdef EnablePaint
uniform sampler2D _SprayTexture;
#endif

//////////////////////////////////////////////////
// SHADER DATA
//////////////////////////////////////////////////

struct Vert
{
	float4 pos;
	float3 normal;
	float3 uv;
	float discardTime;
};
uniform StructuredBuffer<Vert> _Buffer;


struct v2f
{
	float3 uv : TEXCOORD0;
	float4 pos : SV_POSITION;

	float4 lightData : TEXCOORD1;
	float3 worldPos: TEXCOORD2;
	float3 worldNormal: TEXCOORD3;
	float3 worldRefl: TEXCOORD4;
	float3 worldViewDir: TEXCOORD5;
	float3 camPosTexcoord : TEXCOORD6;
	float4 screenPos:TEXCOORD7;
	float3 viewNormal : TEXCOORD8;
    
    #if defined(EnableFog)
    UNITY_FOG_COORDS(9)
    #endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

#ifdef UNITY_5
	#define unity_ObjectToWorld _Object2World // UNITY_SHADER_NO_UPGRADE
	#define unity_WorldToObject _World2Object // UNITY_SHADER_NO_UPGRADE
	#define UnityObjectToClipPos(_X) mul(UNITY_MATRIX_MVP, float4(_X.xyz, 1.0)) // UNITY_SHADER_NO_UPGRADE
#endif

float3 RotateAroundAxis (float3 position, float3 axis, float degrees)
{
	axis = normalize(axis);
	float alpha = degrees * UNITY_PI / 180.0;
	float sina, cosa;
	sincos(alpha, sina, cosa);
	float3x3 m = float3x3(axis.x*axis.x*(1-cosa)+cosa, axis.y*axis.x*(1-cosa)-axis.z*sina, axis.z*axis.x*(1-cosa)+axis.y*sina,
						  axis.x*axis.y*(1-cosa)+axis.z*sina, axis.y*axis.y*(1-cosa)+cosa, axis.z*axis.y*(1-cosa)-axis.x*sina,
						  axis.x*axis.z*(1-cosa)-axis.y*sina, axis.y*axis.z*(1-cosa)+axis.x*sina, axis.z*axis.z*(1-cosa)+cosa);
	return mul(m,position);
}

//////////////////////////////////////////////////
// VERTEX SHADER
//////////////////////////////////////////////////
v2f vertVolumetric(uint id : SV_VertexID)
{
	Vert vert = _Buffer[id];
	
	float3 localPos = vert.pos;
	if(vert.discardTime>0)
	{
		float time = _Time.y-vert.discardTime;
		localPos = RotateAroundAxis(vert.pos,float3(1,0.4,0),60*time);
		localPos+=float3(0,3,3)*time + (float3(0,-35.0f,0)*time*time)/2.0f;
	}
	float3 worldPos = mul(unity_ObjectToWorld, vert.pos).xyz;
	float3 worldNormal = normalize(mul(unity_ObjectToWorld, float4(vert.normal, 0.0)).xyz);
	float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));

	// CALCULATE TANGENT SPACE
	float3 projNormal = saturate(pow(worldNormal*1.5, 4));
 	float3 tangent = float3(0,0,0);
	tangent.xyz = lerp(tangent.xyz, float3(0, 0, 1), projNormal.y);
	tangent.xyz = lerp(tangent.xyz, float3(0, 1, 0), projNormal.x);
	tangent.xyz = lerp(tangent.xyz, float3(1, 0, 0), projNormal.z);
	tangent.xyz = tangent.xyz - dot(tangent.xyz, worldNormal) * worldNormal;
	tangent.xyz = normalize(tangent.xyz);
	
	// texture space (TBN) basis-vector
	float3 binormal = cross(tangent.xyz, vert.normal);
	float3x3 tbn = float3x3(tangent.xyz, binormal, vert.normal);

	// get cam pos in texture (TBN) space
	float3 camPosLocal = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0)).xyz;
	float3 dirToCamLocal = camPosLocal - localPos;
	float3 camPosTexcoord = mul(tbn, dirToCamLocal);

	v2f o;

    UNITY_INITIALIZE_OUTPUT(v2f, o);
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.pos = UnityObjectToClipPos(localPos);
	o.uv = vert.uv;
	o.worldNormal = worldNormal;
	o.worldRefl = reflect(-worldViewDir, worldNormal);
	o.worldPos = worldPos;
	o.worldViewDir = worldViewDir;
	o.camPosTexcoord = camPosTexcoord;
	o.screenPos = ComputeScreenPos(o.pos);
	o.viewNormal = normalize(mul(UNITY_MATRIX_MV, float4(vert.normal, 0.0)).xyz);

    #if defined(EnableFog)
    UNITY_TRANSFER_FOG(o, o.pos);
    #endif

    return o;
}

inline fixed3 GetTriplanarColor(float3 worldNormal, float3 uv, sampler2D _Texture, float4 _Texture_ST)
{
	float3 normal = abs(worldNormal);
	normal/=dot(normal,1.0);

	float3 colorXZ = tex2D(_Texture,uv.xz * _Texture_ST.xy + _Texture_ST.zw).rgb;
	float3 colorYZ = tex2D(_Texture,uv.yz * _Texture_ST.xy + _Texture_ST.zw).rgb;
	float3 colorXY = tex2D(_Texture,uv.xy * _Texture_ST.xy + _Texture_ST.zw).rgb;

	return colorYZ*normal.x + colorXZ*normal.y +colorXY*normal.z;
}

//////////////////////////////////////////////////
// FRAGMENT SHADER
//////////////////////////////////////////////////
fixed4 fragVolumetric(v2f i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i );
    float phong = saturate(dot(i.worldNormal, normalize(_WorldSpaceCameraPos - i.worldPos)));
	
	// height map UV (marble texture)
	float3 uvMarble = i.uv;

	// caustic sampling
# ifdef EnableCaustic
	float caustic = GetTriplanarColor(i.worldNormal,i.uv + float3(0.0, _Time.x*_CausticScrollSpeed,0.0),_CausticMap,_CausticMap_ST).r;
	uvMarble += float3(caustic, _Time.x,0) * _MarbleHeightCausticOffset;
#endif

    // height-field offset
    float3 eyeVec = normalize(i.camPosTexcoord);
	float height = GetTriplanarColor(i.worldNormal,uvMarble,_MarbleTex,_MarbleTex_ST).r;
	float v = height * _MarbleHeightScale - (_MarbleHeightScale*0.5);
	float3 newCoords = i.uv + eyeVec * v;

	// accumulate layers
	float3 colorLayersAccum = float3(0.0, 0.0, 0.0);
	float layerDepthFalloffAccum = 1.0;
	float layerHeightBiasAccum = _LayerHeightBias;

	// layer 0
	#ifdef EnableLayer0
	{
		float3 layerBaseUV =i.uv + _Time.x * float3(_Layer0SpeedX,_Layer0SpeedY,0);
		float3 layerParallaxUV = layerBaseUV + eyeVec * v + eyeVec * -layerHeightBiasAccum;

		colorLayersAccum += GetTriplanarColor(i.worldNormal,layerParallaxUV,_Layer0Tex,_Layer0Tex_ST) * layerDepthFalloffAccum * _Layer0Tint.xyz;
		layerDepthFalloffAccum *= _LayerDepthFalloff;
		layerHeightBiasAccum += _LayerHeightBiasStep;
	}
	#endif

	// layer 1
	#ifdef EnableLayer1
	{
		float3 layerBaseUV = i.uv+ _Time.x * float3(_Layer1SpeedX, _Layer1SpeedY,0);
		float3 layerParallaxUV = layerBaseUV + eyeVec * v + eyeVec * -layerHeightBiasAccum;

		colorLayersAccum += GetTriplanarColor(i.worldNormal,layerParallaxUV,_Layer1Tex,_Layer1Tex_ST) * layerDepthFalloffAccum * _Layer1Tint.xyz;
		layerDepthFalloffAccum *= _LayerDepthFalloff;
		layerHeightBiasAccum += _LayerHeightBiasStep;
	}
	#endif

	// layer 2
	#ifdef EnableLayer2
	{
		float3 layerBaseUV = i.uv + _Time.x * float3(_Layer2SpeedX, _Layer2SpeedY,0);
		float3 layerParallaxUV = layerBaseUV + eyeVec * v + eyeVec * -layerHeightBiasAccum;

		colorLayersAccum += GetTriplanarColor(i.worldNormal,layerParallaxUV,_Layer2Tex,_Layer2Tex_ST) * layerDepthFalloffAccum * _Layer2Tint.xyz;
		layerDepthFalloffAccum *= _LayerDepthFalloff;
		layerHeightBiasAccum += _LayerHeightBiasStep;
	}
	#endif

	float3 color = colorLayersAccum;
	float alpha = 0.0;

	// marble
	fixed3 texMarble = GetTriplanarColor(i.worldNormal,newCoords,_MarbleTex,_MarbleTex_ST);
	color += texMarble.xyz;
	//fixed4 texMarble2 = tex2D(_MarbleTex, newCoords + i.uv);
	//color += texMarble2.xyz;

	// alpha everything so far
	alpha += saturate(lumin(color));

	// fresnel
	#ifdef EnableFresnel
	float fresnel = pow(1.0 - phong, _FresnelTightness);
														
	// fresnel - reflections use skybox?
	#ifdef EnableFresnelUseSkybox
	half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.worldRefl);
	half3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);
	color += lerp(_FresnelColorInside, _FresnelColorOutside, fresnel) * skyColor.xyz * fresnel;
	#else
	color += lerp(_FresnelColorInside, _FresnelColorOutside, fresnel) * fresnel;
	#endif

	alpha += fresnel;
	#endif

	// inner light
	#ifdef EnableInnerLight
	float innerLight = pow(phong, _InnerLightTightness);
	color += lerp(_InnerLightColorOutside, _InnerLightColorInside, innerLight) * innerLight;
	alpha += innerLight;
	#endif

	// caustic
	#ifdef EnableCaustic
	color += _CausticTint.xyz * caustic;
	alpha += caustic*_CausticTint.w;
	#endif

	// overall alpha mask
	#ifdef EnableSurfaceMask
	float alphaMask = tex2D(_SurfaceAlphaMaskTex, TRANSFORM_TEX(i.uv, _SurfaceAlphaMaskTex)).r;
	color = color + _SurfaceAlphaColor.xyz * alphaMask;
	alpha += alphaMask;
	#endif

	// specular
	#ifdef EnableSpecular
	float3 worldNormalNormalized = normalize(i.worldNormal);
	float3 R = reflect(-_WorldSpaceLightPos0.xyz, worldNormalNormalized);
	float specular = pow(saturate(dot(R, normalize(i.worldViewDir))), _SpecularTightness);
	color += _LightColor0.xyz * specular * _SpecularBrightness;
	alpha += specular * _SpecularBrightness;
	#endif

	color = saturate(color);
	alpha = saturate(alpha);

	// refraction/distortion
	#if defined(EnableRefraction) && !defined(VOLUMETRIC_MOBILE_VER)
	float2 screenUV = i.screenPos.xy / i.screenPos.w;
	//half4 bgcolor = tex2D(_BackgroundTexture, screenUV + (i.worldNormal.xy*0.5+float2(height, 0.0))*_RefractionStrength);
	half4 bgcolor = tex2D(_BackgroundTexture, screenUV + (-i.viewNormal.xy*0.5+float2(height, 0.0))*_RefractionStrength);
	float newAlpha = tex2D(_OpacityTexture,i.uv).a;
	color = bgcolor.rgb*newAlpha + color*(1-newAlpha);
	alpha = 1.0;
	#else
	alpha = 1-tex2D(_OpacityTexture,i.uv).a;
	#endif

    #if defined(EnableFog)
    UNITY_APPLY_FOG(i.fogCoord, color);
    #endif

	#ifdef EnablePaint
	float3 absNormal = abs(i.worldNormal);
	if(absNormal.z>absNormal.y && absNormal.z>absNormal.x) //if only front or back face then apply spray paint
	{
		//todo: optimize not always necessary steps
		float4 sprayColor = tex2D(_SprayTexture,i.uv);
		sprayColor*=GetRemappedUVColor(i.uv); //clip spray by sticker decor mask
	
		//"alpha" blending with transparent background and brush (fix hard edges through time)
		float outAlpha = sprayColor.a+alpha*(1-sprayColor.a);
		color = (sprayColor.rgb+color*alpha*(1-sprayColor.a))/outAlpha;
		alpha = outAlpha;
	}
	#endif
	
	return float4(color,alpha);
}

#endif