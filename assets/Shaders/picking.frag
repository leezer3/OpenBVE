#version 150 core
uniform int uObjectIndex;
out vec4 fragColor;

void main(void)
{
	// Write object index to red channel and write 0/1 to other channels to avoid undefined behavior
	fragColor = vec4(float(uObjectIndex), 0.0, 0.0, 1.0);
}
