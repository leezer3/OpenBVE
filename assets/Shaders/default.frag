#version 150
in vec4 oViewPos;
in vec2 oUv;
in vec4 oColor;
in vec4 oLightResult;
uniform bool uAlphaTestEnabled;
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

	if(uAlphaTestEnabled)
	{
		switch(uAlphaFunction)
		{
			case 512: // Never
				discard;
			case 513: // Less
				if(finalColor.a >= uAlphaComparison)
				{
					discard;
				}
				break;
			case 514: // Equal
				if(!(abs(finalColor.a - 1.0) < 0.00001))
				{
					discard;
				}
				break;
			case 515: // LEqual
				if(finalColor.a > uAlphaComparison)
				{
					discard;
				}
				break;
			case 516: // Greater
				if(finalColor.a <= uAlphaComparison)
				{
					discard;
				}
				break;
			case 517: // NotEqual
				if((abs(finalColor.a - 1.0) < 0.00001))
				{
					discard;
				}
				break;
			case 518: // GEqual
				if(finalColor.a < uAlphaComparison)
				{
					discard;
				}
				break;
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
