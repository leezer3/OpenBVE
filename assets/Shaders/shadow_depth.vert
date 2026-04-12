#version 410 core

layout(location = 0) in vec3 iPosition;
layout(location = 2) in vec2 iUv;
layout(location = 4) in ivec3 iMatrixChain;

uniform mat4 uLightSpaceMatrix;
uniform mat4 uModelMatrix;

out vec2 vUv;

layout(std140) uniform matrices
{
	mat4 uMatrix[128];
};

vec3 transformVector(vec3 vector, int index)
{
	return vec3(uMatrix[index] * vec4(vector, 1.0));
}

void main()
{
	vec3 pos = iPosition;
	
	if(iMatrixChain.x != 0)
	{	
		int m0 = (iMatrixChain.x & (0xff << 24)) >> 24;
		int m1 = (iMatrixChain.x >> 16) & 0xff;
		int m2 = (iMatrixChain.x & 0xff00) >> 8;
		int m3 = (iMatrixChain.x & 0xff);
        
		if(m0 >=0 && m0 < 255) pos = transformVector(pos, m0);
		if(m1 >=0 && m1 < 255) pos = transformVector(pos, m1);
		if(m2 >=0 && m2 < 255) pos = transformVector(pos, m2);
		if(m3 >=0 && m3 < 255) pos = transformVector(pos, m3);
	}
	
	if(iMatrixChain.y != 0)
	{	
		int m0 = (iMatrixChain.y & (0xff << 24)) >> 24;
		int m1 = (iMatrixChain.y >> 16) & 0xff;
		int m2 = (iMatrixChain.y & 0xff00) >> 8;
		int m3 = (iMatrixChain.y & 0xff);
        
		if(m0 >=0 && m0 < 255) pos = transformVector(pos, m0);
		if(m1 >=0 && m1 < 255) pos = transformVector(pos, m1);
		if(m2 >=0 && m2 < 255) pos = transformVector(pos, m2);
		if(m3 >=0 && m3 < 255) pos = transformVector(pos, m3);
	}

	if(iMatrixChain.z != 0)
	{	
		int m0 = (iMatrixChain.z & (0xff << 24)) >> 24;
		int m1 = (iMatrixChain.z >> 16) & 0xff;
		int m2 = (iMatrixChain.z & 0xff00) >> 8;
		int m3 = (iMatrixChain.z & 0xff);
        
		if(m0 >=0 && m0 < 255) pos = transformVector(pos, m0);
		if(m1 >=0 && m1 < 255) pos = transformVector(pos, m1);
		if(m2 >=0 && m2 < 255) pos = transformVector(pos, m2);
		if(m3 >=0 && m3 < 255) pos = transformVector(pos, m3);
	}

	// OpenBVE explicitly negates Z for world geometry.
	pos.z = -pos.z;

	vUv = iUv;
	gl_Position = uLightSpaceMatrix * uModelMatrix * vec4(pos, 1.0);
}
