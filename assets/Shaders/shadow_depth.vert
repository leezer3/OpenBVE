//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2024, Aditiya Afrizal, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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

