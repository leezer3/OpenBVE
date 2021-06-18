#version 150
in vec4 oViewPos;
in vec2 oUv;
in vec4 oColor;
in vec4 oLightResult;
uniform int uAlphaFunction;
uniform float uAlphaComparison;
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
out vec4 fragColor;

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

	/*
	 * NOTES:
	 * Unused alpha functions must not be added to the shader
	 * This has a nasty affect on framerates
	 *
	 * A switch case block is also ~30% slower than the else-if
	 *
	 * Numbers used are those from the GL.AlphaFunction enum to allow
	 * for direct casts
	 */
	if(uAlphaFunction == 513) // Less
	{
		if(finalColor.a >= uAlphaComparison)
		{
			discard;
		}
	}
	else if(uAlphaFunction == 514) // Equal
	{
		if(!(abs(finalColor.a - 1.0) < 0.00001))
		{
			discard;
		}
	}
	else if(uAlphaFunction == 516) // Greater
	{
		if(finalColor.a <= uAlphaComparison)
		{
			discard;
		}
	}
	
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

	fragColor = vec4(mix(uFogColor, finalColor.rgb, fogFactor), finalColor.a);
}
