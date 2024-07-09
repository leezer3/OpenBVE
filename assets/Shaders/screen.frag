#version 150 core

// shader inputs
in vec2 oUv;

// shader outputs
out vec4 fragColor;

// screen image
uniform sampler2D screen;

void main()
{
	fragColor = vec4(texture(screen, oUv).rgb, 1.0f);
}
