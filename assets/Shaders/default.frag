#version 150

in vec2 oUv;
in vec4 oColor;
in float oFogFactor;

uniform vec3  uFogColor;
uniform bool uIsTexture;
uniform sampler2D uTexture;
uniform float uBrightness;
uniform float uOpacity;
uniform int uIsAdditive;

void main(void)
{
	vec4 textureColor = vec4(1.0);

	if(uIsTexture)
	{
		textureColor *= texture2D(uTexture, oUv);
	}

	vec3 finalRGB = vec3(oColor.rgb * textureColor.rgb * uBrightness);
	float finalA = oColor.a * textureColor.a * uOpacity;
	switch(uIsAdditive)
	{
		case 1:
			//Plain additive
			finalRGB /= finalA;
		break;
		case 2:
			//Divide exponent 2
			finalRGB /= finalA * 2;
		break;
		case 3:
			//Divide exponent 4
			finalRGB /= finalA * 4;
		break;
	}
		
	gl_FragData[0] = clamp(vec4(mix(uFogColor, finalRGB, oFogFactor), oColor.a * textureColor.a * uOpacity), 0.0, 1.0);
}
