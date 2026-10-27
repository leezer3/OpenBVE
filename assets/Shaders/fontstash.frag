#version 150 core

in vec4 vColor;
in vec2 vUv;

uniform sampler2D uTexture;

out vec4 fragColor;

void main()
{
	fragColor = vColor * texture(uTexture, vUv);
}
