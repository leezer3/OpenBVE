#version 150 core
in vec2 textureCoord;
uniform int uAlphaFunction;
uniform float uAlphaComparison;
uniform bool uRectangleHasColour;
uniform vec4 uColor;
uniform sampler2D uTexture;
out vec4 fragColor;

void main(void)
{
	vec4 textureColour = texture(uTexture, textureCoord);
	
	vec4 finalColor = textureColour * uColor;

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
		if(finalColor.a > uAlphaComparison)
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
	
	fragColor = finalColor;
	
}
