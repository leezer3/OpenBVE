#version 150
uniform int uObjectIndex;
out vec4 fragColor;

void main(void)
{
	fragColor.r = float(uObjectIndex);
}
