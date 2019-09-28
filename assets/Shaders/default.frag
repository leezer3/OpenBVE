#version 130

in vec2 oUv;
in vec4 oColor;
in float oFogFactor;

uniform vec4  uFogColor;
uniform bool uIsTexture;
uniform sampler2D uTexture;
uniform float uBrightness;
uniform float uOpacity;

void main(void)
{
	vec4 textureColor = vec4(1.0);

	if(uIsTexture)
	{
		textureColor *= texture2D(uTexture, oUv);
	}

	gl_FragData[0] = clamp(vec4(mix(vec3(uFogColor), vec3(oColor.rgb * textureColor.rgb * uBrightness), oFogFactor), oColor.a * textureColor.a * uOpacity), 0.0, 1.0);
}
