#version 150
uniform mat4 uCurrentProjectionMatrix;
uniform mat4 uCurrentModelViewMatrix;
uniform vec2 uPoint;
uniform vec2 uSize;
uniform vec2 uCoordinates;
out vec2 textureCoord;
vec4 viewPos = vec4(0,0,0,0);

void main()
{
	if(gl_VertexID == 0)
	{
		viewPos = uCurrentModelViewMatrix * vec4(vec3(uPoint.x, uPoint.y, 0), 1.0);
		textureCoord = vec2(0,0);
	}
	else if (gl_VertexID == 1)
	{
		viewPos = uCurrentModelViewMatrix * vec4(vec3(uPoint.x + uSize.x, uPoint.y, 0), 1.0);
		textureCoord = vec2(uCoordinates.x,0);
	}
	else if (gl_VertexID == 2)
	{
		viewPos = uCurrentModelViewMatrix * vec4(vec3(uPoint.x + uSize.x, uPoint.y + uSize.y, 0), 1.0);
		textureCoord = uCoordinates;
	}
	else if (gl_VertexID == 3)
	{
		viewPos = uCurrentModelViewMatrix * vec4(vec3(uPoint.x, uPoint.y, 0), 1.0);
		textureCoord = vec2(0,0);
	}
	else if (gl_VertexID == 4)
	{
		viewPos = uCurrentModelViewMatrix * vec4(vec3(uPoint.x, uPoint.y + uSize.y, 0), 1.0);
		textureCoord = vec2(0, uCoordinates.y);
	}
	else if (gl_VertexID == 5)
	{
		viewPos = uCurrentModelViewMatrix * vec4(vec3(uPoint.x + uSize.x, uPoint.y + uSize.y, 0), 1.0);
		textureCoord = uCoordinates;
	}

	gl_Position = uCurrentProjectionMatrix * viewPos;
}
