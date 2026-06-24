#version 150 core
in vec2 textureCoord;
in vec2 fragOffset;
uniform vec2 uAlphaTest;
uniform bool uRectangleHasColour;
uniform vec4 uColor;
uniform sampler2D uTexture;
uniform vec2 uSize;
uniform float uCornerRadius;
out vec4 fragColor;

float roundSDF(vec2 p, vec2 b, float r) 
{
    vec2 q = abs(p) - b + r;
    return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
}

void main(void)
{
	vec4 textureColour = texture(uTexture, textureCoord);
	vec4 finalColor = textureColour * uColor;

	if (uCornerRadius > 0.0)
	{
		vec2 halfSize = uSize * 0.5;
		vec2 pos = (fragOffset - 0.5) * uSize;
		float dist = roundSDF(pos, halfSize, uCornerRadius);
		
		float alpha = 1.0 - smoothstep(-0.5, 0.5, dist);
		if (alpha <= 0.0)
		{
			discard;
		}
		finalColor.a *= alpha;
	}

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
	if(uAlphaTest.x == 513) // Less
	{
		if(finalColor.a > uAlphaTest.y)
		{
			discard;
		}
	}
	else if(uAlphaTest.x == 514) // Equal
	{
		if(!(abs(finalColor.a - 1.0) < 0.00001))
		{
			discard;
		}
	}
	else if(uAlphaTest.x == 516) // Greater
	{
		if(finalColor.a <= uAlphaTest.y)
		{
			discard;
		}
	}
	
	fragColor = finalColor;
	
}
