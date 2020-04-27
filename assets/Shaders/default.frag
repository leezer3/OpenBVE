#version 150

in vec4 oViewPos;
in vec2 oUv;
in vec4 oColor;

uniform bool uIsTexture;
uniform sampler2D uTexture;
uniform float uBrightness;
uniform float uOpacity;
uniform bool uIsFog;
uniform float uFogStart;
uniform float uFogEnd;
uniform vec3  uFogColor;

void main(void)
{
	vec4 textureColor = vec4(1.0);

	if(uIsTexture)
	{
		textureColor *= texture2D(uTexture, oUv);
	}

	vec3 finalRGB = vec3(oColor.rgb * textureColor.rgb * uBrightness);
	float finalA = oColor.a * textureColor.a * uOpacity;
	finalRGB *= finalA;
		
	// Fog
	float fogFactor = 1.0;

	if (uIsFog)
	{
		fogFactor *= clamp((uFogEnd - length(oViewPos)) / (uFogEnd - uFogStart), 0.0, 1.0);
	}

	gl_FragData[0] = clamp(vec4(mix(uFogColor, finalRGB, fogFactor), finalA), 0.0, 1.0);
}
