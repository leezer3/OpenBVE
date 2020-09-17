#version 150

in vec4 oViewPos;
in vec2 oUv;
in vec4 oColor;
in vec4 oLightResult;

uniform bool uIsTexture;
uniform sampler2D uTexture;
uniform int uMaterialFlags;
uniform float uBrightness;
uniform float uOpacity;
uniform bool uIsFog;
uniform float uFogStart;
uniform float uFogEnd;
uniform vec3  uFogColor;
uniform float uFogDensity;
uniform bool uFogIsLinear;

void main(void)
{
	vec4 finalColor = vec4(oColor.rgb, 1.0);
	if(uIsTexture)
	{
		finalColor *= texture2D(uTexture, oUv);
	}
	if((uMaterialFlags & 1) == 0 && (uMaterialFlags & 4) == 0)
	{
		//Material is not emissive and lighting is enabled, so multiply by brightness
		finalColor.rgb *= uBrightness;
	}
	finalColor.a *= uOpacity;
	//Apply the lighting results *after* the final color has been calculated
	finalColor *= oLightResult;

	// Fog
	float fogFactor = 1.0;

	if (uIsFog)
	{
		if(uFogIsLinear)
		{
			fogFactor = clamp((uFogEnd - length(oViewPos)) / (uFogEnd - uFogStart), 0.0, 1.0);
		}
		else
		{
			fogFactor = exp(-pow(uFogDensity * (gl_FragCoord.z / gl_FragCoord.w), 2.0));
		}
	}

	gl_FragData[0] = vec4(mix(uFogColor, finalColor.rgb, fogFactor), finalColor.a);
}
