#version 150 core

in vec3 iPosition;
in vec2 iUv;
out vec2 oUv;

void main()
{
	oUv = iUv;
	gl_Position = vec4(iPosition, 1.0f);
}
