#version 150 core
in vec2 textureCoord;
uniform vec2 uAlphaTest;
uniform bool uRectangleHasColour;
uniform vec4 uColor;
uniform vec2 uSize;
uniform float uRadius;
uniform vec4 uCorners;
uniform sampler2D uTexture;
out vec4 fragColor;

void main(void)
{
	vec4 textureColour = texture(uTexture, textureCoord);
	
	vec4 finalColor = textureColour * uColor;

	// Rounded corners: SDF discard with a 1px feathered edge.
	// uRadius defaults to 0 (square); callers opt in via DrawRounded.
	// uCorners selects rounded corners (x=TL y=TR z=BR w=BL, 1=round 0=square).
	// Local position is recovered from the UVs, which span 0..1 for plain fills.
	if(uRadius > 0.5)
	{
		vec2 localPos = textureCoord * uSize;
		vec2 halfSize = uSize * 0.5;
		vec2 side = step(halfSize, localPos);
		float mask = mix(mix(uCorners.x, uCorners.y, side.x), mix(uCorners.w, uCorners.z, side.x), side.y);
		float r = min(uRadius * mask, min(uSize.x, uSize.y) * 0.5);
		vec2 q = abs(localPos - halfSize) - halfSize + vec2(r);
		float dist = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
		if(dist > 1.0)
		{
			discard;
		}
		finalColor.a *= clamp(1.0 - dist, 0.0, 1.0);
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
