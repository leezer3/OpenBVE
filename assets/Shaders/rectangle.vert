#version 150 core
uniform mat4 uCurrentProjectionMatrix;
uniform mat4 uCurrentModelViewMatrix;
uniform vec2 uPoint;
uniform vec2 uSize;
uniform vec2 uCoordinates;
uniform vec2 uRotationOrigin;
uniform float uRotation;
out vec2 textureCoord;
vec4 viewPos = vec4(0,0,0,0);

vec2 rotateScreenPoint(vec2 p)
{
	vec2 d = p - uRotationOrigin;
	float c = cos(uRotation);
	float s = sin(uRotation);
	return uRotationOrigin + vec2(d.x * c - d.y * s, d.x * s + d.y * c);
}

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

	if(uRotation != 0.0)
	{
		viewPos.xy = rotateScreenPoint(viewPos.xy);
	}
	gl_Position = uCurrentProjectionMatrix * viewPos;
}
