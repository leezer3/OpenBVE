#version 150
in vec2 textureCoord;
uniform int uAlphaFunction;
uniform float uAlphaComparison;
uniform bool uIsTexture;
uniform bool uRectangleHasColour;
uniform vec4 uColor;
uniform sampler2D uTexture;
out vec4 fragColor;

void main(void)
{
	vec4 textureColour = vec4(1.0,1.0,1.0,1.0);
	if(uIsTexture)
	{
		textureColour *= texture(uTexture, textureCoord);
	}
	vec4 finalColor = textureColour * uColor;
	switch(uAlphaFunction)
	{
		/*
		* NOTE:
		* Unused alpha functions must not be added to the shader
		* This has a nasty affect on framerates
		*/
		//case 512: // Never
		//	discard;
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
		//case 515: // LEqual
		//	if(finalColor.a > uAlphaComparison)
		//	{
		//		discard;
		//	}
		//	break;
		case 516: // Greater
			if(finalColor.a <= uAlphaComparison)
			{
				discard;
			}
			break;
		//case 517: // NotEqual
		//	if((abs(finalColor.a - 1.0) < 0.00001))
		//	{
		//		discard;
		//	}
		//	break;
		//case 518: // GEqual
		//	if(finalColor.a < uAlphaComparison)
		//	{
		//		discard;
		//	}
		//	break;
	}
	fragColor = finalColor;
	
}
