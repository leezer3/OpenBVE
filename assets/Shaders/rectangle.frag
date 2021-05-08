#version 150
in vec2 textureCoord;
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
	fragColor = textureColour * uColor;
	
}
