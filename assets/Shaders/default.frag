//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2024, Christopher Lees, S520, Aditiya Afrizal, The OpenBVE Project
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
precision highp float;
in vec4 oViewPos;
in vec2 oUv;
in vec4 oColor;
in vec4 oLightResult;
uniform sampler2DShadow uShadowMap0;  // Cascade 0 (near)
uniform sampler2DShadow uShadowMap1;  // Cascade 1 (mid)
uniform sampler2DShadow uShadowMap2;  // Cascade 2 (far)
uniform sampler2DShadow uShadowMap3;  // Cascade 3 (extra far)
uniform float uCascadeFarDist0;
uniform float uCascadeFarDist1;
uniform float uCascadeFarDist2;
uniform float uCascadeFarDist3;
uniform float uCascadeBias0;
uniform float uCascadeBias1;
uniform float uCascadeBias2;
uniform float uCascadeBias3;
uniform float uNormalBias0;
uniform float uNormalBias1;
uniform float uNormalBias2;
uniform float uNormalBias3;
uniform float uShadowStrength;
uniform bool  uShadowEnabled;
uniform int   uCascadeCount;  // 2, 3, or 4
uniform vec2  uAlphaTest;
uniform sampler2D uTexture;
uniform sampler2D uNightTexture;
uniform bool uIsNightTexture;
uniform float uNightBlendFactor;

struct Light
{
	vec3 position;
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
	vec4 lightModel;
};
uniform Light uLight;

// Inputs from vertex shader
in vec3  vNormal;
in vec4  vPosLightSpace0;
in vec4  vPosLightSpace1;
in vec4  vPosLightSpace2;
in vec4  vPosLightSpace3;
in float vViewDepth;
uniform int uMaterialFlags;
uniform float uBrightness;
uniform float uOpacity;
uniform bool uIsFog;
uniform float uFogStart;
uniform float uFogEnd;
uniform vec3  uFogColor;
uniform float uFogDensity;
uniform bool uFogIsLinear;
out vec4 fragColor;

/// Samples a single cascade with 4-tap PCF via hardware comparison.
float SampleCascade(sampler2DShadow shadowMap, vec4 posLightSpace, float bias, float normalBias)
{
    vec3 projCoords = posLightSpace.xyz / posLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;

    // Out-of-bounds check
    if (projCoords.x < 0.0 || projCoords.x > 1.0 ||
        projCoords.y < 0.0 || projCoords.y > 1.0 ||
        projCoords.z < 0.0 || projCoords.z > 1.0)
    {
        return 1.0;
    }

    // Compute slope-scaled Z-bias dynamically based on the exact texel size fraction passed from C#.
    vec3 normal = normalize(vNormal);
    vec3 lightDir = normalize(uLight.position);
    float biasScale = clamp(1.0 - dot(normal, lightDir), 0.0, 1.0);
    // Multiply the base Z-bias by a slope factor to perfectly cure acne on thin meshes
    float activeBias = bias * (1.1 + biasScale * normalBias); 

    float currentDepth = projCoords.z - activeBias;

    // Tight 4-tap rotated grid PCF for sharper shadows.
    // Each tap is bilinear-averaged by the hardware sampler2DShadow.
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    float shadow = 0.0;
    
    // Rotated grid offsets at a tight 0.5 texels
    shadow += texture(shadowMap, vec3(projCoords.xy + vec2(-0.5, -0.5) * texelSize, currentDepth));
    shadow += texture(shadowMap, vec3(projCoords.xy + vec2( 0.5, -0.5) * texelSize, currentDepth));
    shadow += texture(shadowMap, vec3(projCoords.xy + vec2(-0.5,  0.5) * texelSize, currentDepth));
    shadow += texture(shadowMap, vec3(projCoords.xy + vec2( 0.5,  0.5) * texelSize, currentDepth));
    shadow *= 0.25;

    return shadow;
}

float SampleCascadeByIndex(int idx)
{
    if (idx == 0) return SampleCascade(uShadowMap0, vPosLightSpace0, uCascadeBias0, uNormalBias0);
    if (idx == 1) return SampleCascade(uShadowMap1, vPosLightSpace1, uCascadeBias1, uNormalBias1);
    if (idx == 2) return SampleCascade(uShadowMap2, vPosLightSpace2, uCascadeBias2, uNormalBias2);
    if (idx == 3) return SampleCascade(uShadowMap3, vPosLightSpace3, uCascadeBias3, uNormalBias3);
    return 1.0;
}

float GetCascadeFarDist(int idx)
{
    if (idx == 0) return uCascadeFarDist0;
    if (idx == 1) return uCascadeFarDist1;
    if (idx == 2) return uCascadeFarDist2;
    if (idx == 3) return uCascadeFarDist3;
    return 0.0;
}

/// Full CSM sampling with cascade selection and smooth blending.
float CSMShadow()
{
    float blendRange = 15.0;
    float shadow = 1.0;
    int cascadeCount = uCascadeCount;

    for (int i = 0; i < cascadeCount; i++)
    {
        float farDist = GetCascadeFarDist(i);

        if (vViewDepth < farDist)
        {
            shadow = SampleCascadeByIndex(i);

            // Blend toward next cascade near the boundary
            if (i < cascadeCount - 1)
            {
                float blendStart = farDist - blendRange;
                if (vViewDepth > blendStart)
                {
                    float nextShadow = SampleCascadeByIndex(i + 1);
                    float t = (vViewDepth - blendStart) / blendRange;
                    shadow = mix(shadow, nextShadow, t);
                }
            }
            else
            {
                // Last cascade: fade out at far edge
                float fadeStart = farDist - blendRange * 2.0;
                if (vViewDepth > fadeStart)
                {
                    float t = (vViewDepth - fadeStart) / (farDist - fadeStart);
                    shadow = mix(shadow, 1.0, t);
                }
            }

            break;  // Found our cascade, stop searching
        }
    }

    return mix(1.0, shadow, uShadowStrength);
}

void main(void)
{
	vec4 finalColor;
	if((uMaterialFlags & 16) == 0)
	{
		finalColor = vec4(oColor.rgb, 1.0) * texture(uTexture, oUv); // NOTE: only want the RGB of the color, A is passed in as part of opacity
	}
	else
	{
		// disable alpha channel when rendering texture (MSTS shape)
		finalColor = vec4(oColor.rgb, 1.0) * vec4(texture(uTexture, oUv).xyz, 1.0);
	}

	if((uMaterialFlags & 1) == 0 && (uMaterialFlags & 4) == 0)
	{
		//Material is not emissive and lighting is enabled, so multiply by brightness
		finalColor.rgb *= uBrightness;
	}
	
	// Multiply material alpha by it's opacity
	finalColor.a *= uOpacity;

	/*
	 * NOTES:
	 * Unused alpha functions must not be added to the shader
	 * This has a nasty affect on framerates
	 *
	 * A switch case block is also ~30% slower than the else-if
	 *
	 * Numbers used are those from the GL.AlphaFunction enum to allow
	 * for direct casts
	 */
	if(uAlphaTest.x == 513) // Less
	{
		if(finalColor.a >= uAlphaTest.y)
		{
			discard;
		}
	}
	else if(uAlphaTest.x == 514) // Equal
	{
		if(!(abs(finalColor.a - uAlphaTest.y) < 0.00001))
		{
			discard;
		}
	}
	else if(uAlphaTest.x == 516) // Greater
	{
		if(finalColor.a <= uAlphaTest.y)
		{
			discard;
		}
	}
		
	/*
	 * Apply the lighting results *after* the final color has been calculated
	 * This *must* also be done after the discard check to get correct results,
	 * as otherwise light coming through a semi-transparent material will 
	 * affect it's final opacity, and hence whether its discarded or not
	 */
	float shadow = 1.0;
	if (uShadowEnabled)
	{
		shadow = CSMShadow();
	}
	
	if ((uMaterialFlags & 1) == 0 && (uMaterialFlags & 4) == 0)
	{
		// Material is not emissive, apply shadow to the light factor
		finalColor.rgb *= (oLightResult.rgb * shadow);
		finalColor.a *= oLightResult.a;

		if (uIsNightTexture)
		{
			vec4 nightColor;
			if ((uMaterialFlags & 16) == 0)
			{
				nightColor = vec4(oColor.rgb, 1.0) * texture(uNightTexture, oUv);
			}
			else
			{
				nightColor = vec4(oColor.rgb, 1.0) * vec4(texture(uNightTexture, oUv).xyz, 1.0);
			}
			// Nighttime textures in OpenBVE are usually blended based on DNB (DaytimeNighttimeBlend)
			finalColor.rgb = mix(finalColor.rgb, nightColor.rgb, uNightBlendFactor);
			finalColor.a = mix(finalColor.a, nightColor.a, uNightBlendFactor);
		}
	}
	else
	{
		finalColor *= oLightResult;
	}
	
	// Fog
	float fogFactor = 1.0;

	if (uIsFog)
	{
		if(uFogIsLinear)
		{
			fogFactor = clamp((uFogEnd - length(oViewPos)) / (uFogEnd - uFogStart), 0.0, 1.0);
		}
		else
		{
			fogFactor = exp(-pow(uFogDensity * (gl_FragCoord.z / gl_FragCoord.w), 2.0));
		}
	}

	fragColor = vec4(mix(uFogColor, finalColor.rgb, fogFactor), finalColor.a);
}
