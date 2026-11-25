#version 150 core

in vec3 iPosition;
in vec2 iUv;
in vec4 iColor;

uniform mat4 uCurrentProjectionMatrix;
uniform mat4 uCurrentModelViewMatrix;

out vec4 vColor;
out vec2 vUv;

void main()
{
	vColor = iColor;
	vUv = iUv;
	gl_Position = uCurrentProjectionMatrix * uCurrentModelViewMatrix * vec4(iPosition, 1.0);
}
