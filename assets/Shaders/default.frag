#version 130

in vec2 oUv;
in vec4 oColor;
in float oFogFactor;

uniform vec3  uFogColor;
uniform bool uIsTexture;
uniform sampler2D uTexture;
uniform float uBrightness;
uniform float uOpacity;
uniform bool uIsAdditive;

void main(void)
{
	vec4 textureColor = vec4(1.0);

	if(uIsTexture)
	{
		textureColor *= texture2D(uTexture, oUv);
	}

	vec3 finalRGB = vec3(oColor.rgb * textureColor.rgb * uBrightness);
	float finalA = oColor.a * textureColor.a * uOpacity;
	if(uIsAdditive)
	{
		finalRGB /= finalA * 2;
	}
		
	gl_FragData[0] = clamp(vec4(mix(uFogColor, finalRGB, oFogFactor), oColor.a * textureColor.a * uOpacity), 0.0, 1.0);
}
