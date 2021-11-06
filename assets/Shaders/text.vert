#version 150 core

uniform mat4 uCurrentProjectionMatrix;
uniform mat4 uCurrentModelViewMatrix;
uniform vec2 uPoint;
uniform vec2 uSize;
uniform vec4 uAtlasLocation;
out vec2 textureCoord;
vec4 viewPos = vec4(0,0,0,0);

void main()
{
	switch(gl_VertexID)
	{
		case 0:
			viewPos = uCurrentModelViewMatrix * vec4(vec3(uPoint.x, uPoint.y, 0), 1.0);
			textureCoord = vec2(uAtlasLocation.x, uAtlasLocation.y);
		break;
		case 1:
			viewPos = uCurrentModelViewMatrix * vec4(vec3(uPoint.x + uSize.x, uPoint.y, 0), 1.0);
			textureCoord = vec2(uAtlasLocation.x + uAtlasLocation.z, uAtlasLocation.y);
		break;
		case 2:
			viewPos = uCurrentModelViewMatrix * vec4(vec3(uPoint.x + uSize.x, uPoint.y + uSize.y, 0), 1.0);
			textureCoord = vec2(uAtlasLocation.x + uAtlasLocation.z, uAtlasLocation.y + uAtlasLocation.w);
		break;
		case 3:
			viewPos = uCurrentModelViewMatrix * vec4(vec3(uPoint.x, uPoint.y, 0), 1.0);
			textureCoord = vec2(uAtlasLocation.x, uAtlasLocation.y);
		break;
		case 4:
			viewPos = uCurrentModelViewMatrix * vec4(vec3(uPoint.x, uPoint.y + uSize.y, 0), 1.0);
			textureCoord = vec2(uAtlasLocation.x, uAtlasLocation.y + uAtlasLocation.w);
		break;
		case 5:
			viewPos = uCurrentModelViewMatrix * vec4(vec3(uPoint.x + uSize.x, uPoint.y + uSize.y, 0), 1.0);
			textureCoord = vec2(uAtlasLocation.x + uAtlasLocation.z, uAtlasLocation.y + uAtlasLocation.w);
		break;
	}
	gl_Position = uCurrentProjectionMatrix * viewPos;
}
