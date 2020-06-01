#version 150

in vec4 oViewPos;
in vec2 oUv;
in vec4 oColor;
in vec4 lightResult;

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
	
	vec4 finalColor = vec4(((textureColor.rgb) * 1.0) + (oColor.rgb * (1 - textureColor.a)), textureColor.a * uOpacity);
	//Apply the lighting results *after* the final color has been calculated
	finalColor *= lightResult;
		
	// Fog
	float fogFactor = 1.0;

	if (uIsFog)
	{
		fogFactor *= clamp((uFogEnd - length(oViewPos)) / (uFogEnd - uFogStart), 0.0, 1.0);
	}

	gl_FragData[0] = vec4(mix(uFogColor, finalColor.rgb, fogFactor), finalColor.a);
}
