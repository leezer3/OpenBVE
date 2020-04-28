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
	float finalA;
	vec4 finalColor;
	if(textureColor.a < 1.0)
	{
		finalA = textureColor.a; //store before mix
		finalColor = vec4(mix(oColor, textureColor, textureColor.a)) * uBrightness;
		finalColor.a = finalA * uOpacity;
	}
	else
	{
		finalColor.rgb = oColor.rgb * textureColor.rgb * uBrightness;
		finalColor.a = oColor.a * textureColor.a  * uOpacity;
		finalColor.rgb *= finalColor.a;
	}
	
	
	
	// Fog
	float fogFactor = 1.0;

	if (uIsFog)
	{
		fogFactor *= clamp((uFogEnd - length(oViewPos)) / (uFogEnd - uFogStart), 0.0, 1.0);
	}

	gl_FragData[0] = vec4(mix(uFogColor, finalColor.rgb, fogFactor), finalColor.a);
}
