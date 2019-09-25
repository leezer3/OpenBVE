#version 130

in vec2 oUv;
in vec4 oColor;
in float oFogFactor;

uniform vec4  uFogColor;
uniform bool uIsTexture;
uniform sampler2D uTexture;
uniform float uBrightness;
uniform float uOpacity;
uniform bool uIsAlphaTest;
uniform int uAlphaFuncType;
uniform float uAlphaFuncValue;

bool AlphaTest(float value)
{
	switch (uAlphaFuncType)
	{
		case 0:
			return false;
		case 1:
			return value < uAlphaFuncValue;
		case 2:
			return value == uAlphaFuncValue;
		case 3:
			return value <= uAlphaFuncValue;
		case 4:
			return value > uAlphaFuncValue;
		case 5:
			return value != uAlphaFuncValue;
		case 6:
			return value >= uAlphaFuncValue;
		default:
			return true;
	}
}

void main(void)
{
	vec4 textureColor = vec4(1.0);

	if(uIsTexture)
	{
		textureColor *= texture2D(uTexture, oUv);
	}

	vec4 color = clamp(vec4(mix(vec3(uFogColor), vec3(oColor.rgb * textureColor.rgb * uBrightness), oFogFactor), oColor.a * textureColor.a * uOpacity), 0.0, 1.0);

	if(uIsAlphaTest && !AlphaTest(color.a))
	{
		discard;
	}

	gl_FragData[0] = color;
}
