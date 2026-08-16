#version 150 core
uniform mat4 uCurrentProjectionMatrix;
uniform mat4 uCurrentModelViewMatrix;
uniform vec2 uPoint;
uniform vec2 uSize;
uniform vec2 uCoordinates;
out vec2 textureCoord;
out vec2 fragOffset;

// Offset lookup table to map gl_VertexID to quad corners without branching
const vec2 offsets[6] = vec2[](
	vec2(0.0, 0.0), // Top-left
	vec2(1.0, 0.0), // Top-right
	vec2(1.0, 1.0), // Bottom-right
	vec2(0.0, 0.0), // Top-left (second triangle)
	vec2(0.0, 1.0), // Bottom-left
	vec2(1.0, 1.0)  // Bottom-right
);

void main()
{
	// Calculate vertex position and texture coordinates based on current vertex ID offsets
	vec2 offset = offsets[gl_VertexID];
	vec4 viewPos = uCurrentModelViewMatrix * vec4(uPoint + offset * uSize, 0.0, 1.0);
	textureCoord = offset * uCoordinates;
	fragOffset = offset;
	gl_Position = uCurrentProjectionMatrix * viewPos;
}
