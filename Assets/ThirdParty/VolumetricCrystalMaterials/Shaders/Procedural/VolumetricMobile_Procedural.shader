// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

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

Shader "FunktronicLabs/VolumetricMobile_Procedural"
{
	Properties
	{
		[Header(Layer 1)]
		_Layer0Tex ("Layer 1 Texture", 2D) = "white" {}
		_Layer0Tint("Layer 1 Tint", COLOR) = (1,1,1,1)
		_Layer0SpeedX("Layer 1 Scroll Speed X", Range(-1.0, 1.0)) = 0
		_Layer0SpeedY("Layer 1 Scroll Speed Y", Range(-1.0, 1.0)) = 0

		[Header(Layer 2)]
		[Toggle(EnableLayer1)] _EnableLayer1("Enable", Float) = 1
		_Layer1Tex ("Layer 2 Texture", 2D) = "white" {}
		_Layer1Tint("Layer 2 Tint", COLOR) = (1,1,1,1)
		_Layer1SpeedX("Layer 2 Scroll Speed X", Range(-1.0, 1.0)) = 0
		_Layer1SpeedY("Layer 2 Scroll Speed Y", Range(-1.0, 1.0)) = 0

		[Header(Layer 3)]
		[Toggle(EnableLayer2)] _EnableLayer2("Enable", Float) = 1
		_Layer2Tex ("Layer 3 Texture", 2D) = "white" {}
		_Layer2Tint("Layer 3 Tint", COLOR) = (1,1,1,1)
		_Layer2SpeedX("Layer 3 Scroll Speed X", Range(-1.0, 1.0)) = 0
		_Layer2SpeedY("Layer 3 Scroll Speed Y", Range(-1.0, 1.0)) = 0

		[Header(Layers Global Properties)]
		_LayerHeightBias("Layer Height Start Bias", Range(0.0, 0.2)) = 0.1
		_LayerHeightBiasStep("Layer Height Step", Range(0.0, 0.3)) = 0.1
		_LayerDepthFalloff("Layer Depth Fallofff", Range(0.0, 1.0)) = 0.9

		[Header(Volumetric Marble)]
		_MarbleTex ("Marble Heightmap Texture", 2D) = "black" {}
		_MarbleHeightScale("Marble Height Scale", Range(0.0, 0.5)) = 0.1
		_MarbleHeightCausticOffset("Marble Caustic Offset", Range(-5.0, 5.0)) = 0.1

		[Header(Caustic)]
		[Toggle(EnableCaustic)] _EnableCaustic("Enable", Float) = 0
		_CausticMap("Caustic Map", 2D) = "black" {}
		_CausticTint("Caustic Tint", COLOR) = (1,1,1,1)
		_CausticScrollSpeed("Caustic Scroll Speed X", Range(-5.0, 5.0)) = 1.0

		[Header(Fresnel)]
		[Toggle(EnableFresnel)] _EnableFresnel("Enable", Float) = 1
		[Toggle(EnableFresnelUseSkybox)] _EnableFresnelUseSkybox("Use Skybox Reflection", Float) = 0
		_FresnelTightness("Fresnel Tightness", Range(0.0, 10.0)) = 4.0
		[HDR] _FresnelColorInside("Fresnel Color Inside", COLOR) = (1,1,0.5,1)
		[HDR] _FresnelColorOutside("Fresnel Color Outside", COLOR) = (1,1,1,1)

		[Header(Surface Mask)]
		[Toggle(EnableSurfaceMask)] _EnableSurfaceMask("Enable", Float) = 0
		_SurfaceAlphaMaskTex("Surface Alpha Mask Texture", 2D) = "white" {}
		_SurfaceAlphaColor("Surface Mask Color", COLOR) = (1,1,1,1)

		[Header(Inner Light)]
		[Toggle(EnableInnerLight)] _EnableInnerLight("Enable", Float) = 0
		_InnerLightTightness("Inner Light Tightness", Range(0.0, 40.0)) = 20.0
		[HDR] _InnerLightColorInside("Inner Light Color Inside", COLOR) = (1,1,1,1)
		[HDR] _InnerLightColorOutside("Inner Light Color Outside", COLOR) = (1,1,0,1)

		[Header(Specular)]
		[Toggle(EnableSpecular)] _EnableSpecular("Enable Specular", Float) = 0
		_SpecularTightness("Specular Tightness", Range(0.0, 40.0)) = 2.0
		_SpecularBrightness("Specular Brightness", Range(0.0, 5.0)) = 1.0

		[Header(Fog)]
		[Toggle(EnableFog)] _EnableFog("Enable Fog", Float) = 0
		
		[Header(Paint)]
		[Toggle(EnablePaint)] _EnablePaint("Enable Paint", Float) = 0

		/*[Header(Refraction)]
		[Toggle(EnableRefraction)] _EnableRefraction("Enable Refraction", Float) = 0
		_RefractionStrength("Refraction Strength", Range(0.0, 1.0)) = 0.2*/
	}

	SubShader
	{
		Blend SrcAlpha OneMinusSrcAlpha
		Tags{ "Queue" = "Transparent" "LightMode" = "ForwardBase" } // the specular "_WorldSpaceLightPos0" query only works in forward rendering
		LOD 200

		Pass
		{
			CGPROGRAM

			#pragma shader_feature __ EnableLayer0
			#pragma shader_feature __ EnableLayer1
			#pragma shader_feature __ EnableLayer2
			#pragma shader_feature __ EnableCaustic			
			#pragma shader_feature __ EnableFresnel
			#pragma shader_feature __ EnableFresnelUseSkybox
			#pragma shader_feature __ EnableSurfaceMask
			#pragma shader_feature __ EnableInnerLight
			#pragma shader_feature __ EnableSpecular
			#pragma shader_feature __ EnableFog
			#pragma shader_feature __ EnablePaint
			//#pragma multi_compile __ EnableRefraction

			#pragma vertex vertVolumetric
			#pragma fragment fragVolumetric
			#pragma multi_compile_fog

			#pragma require compute

			#define VOLUMETRIC_MOBILE_VER // NO grabpass for mobile.. too heavy!
			#include "Volumetric_Procedural.cginc"

			ENDCG
		}
	}

	Fallback "VertexLit"
}
