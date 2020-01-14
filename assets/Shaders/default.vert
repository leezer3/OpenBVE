#version 150

struct Light
{
    vec3 position;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

struct MaterialColor
{
    vec4 ambient;
    vec4 diffuse;
    vec4 specular;
	vec3 emission;
    float shininess;
	bool isEmissive;
}; 

in vec3 iPosition;
in vec3 iNormal;
in vec2 iUv;
in vec4 iColor;

uniform mat4 uCurrentProjectionMatrix;
uniform mat4 uCurrentModelViewMatrix;
uniform mat4 uCurrentTextureMatrix;

uniform bool uIsLight;
uniform bool uIsEmissive;
uniform Light uLight;
uniform MaterialColor uMaterial;
uniform bool uIsFog;
uniform float uFogStart;
uniform float uFogEnd;

out vec2 oUv;
out vec4 oColor;
out float oFogFactor;

void main()
{
	vec4 viewPos = uCurrentModelViewMatrix * vec4(vec3(iPosition.x, iPosition.y, -iPosition.z), 1.0);
	vec3 viewNormal = mat3(transpose(inverse(uCurrentModelViewMatrix))) * vec3(iNormal.x, iNormal.y, -iNormal.z);
	gl_Position = uCurrentProjectionMatrix * viewPos;

	// Lighting
	// Ambient
	vec4 ambient  = vec4(uLight.ambient.x, uLight.ambient.y, uLight.ambient.z, 1.0) * uMaterial.ambient;

	// Diffuse
	viewNormal = normalize(viewNormal);
	vec3 eye = normalize(-viewPos.xyz);
	float diff = max(dot(viewNormal, uLight.position), 0.0);
	vec4 diffuse = vec4(uLight.diffuse.x, uLight.diffuse.y, uLight.diffuse.z, 1.0) * (diff * uMaterial.diffuse);

	// Specular
	vec4 specular = vec4(0.0);

	if(diff > 0.0)
	{
		vec3 halfVector = normalize(uLight.position + eye);
		float spec = pow(max(dot(halfVector, viewNormal), 0.0), uMaterial.shininess);
		specular = vec4(uLight.specular.x, uLight.specular.y, uLight.specular.z, 1.0) * (spec * uMaterial.specular);
	}

	// Fog
	oFogFactor = 1.0;

	if (uIsFog)
	{
		oFogFactor *= clamp((uFogEnd - length(viewPos)) / (uFogEnd - uFogStart), 0.0, 1.0);
	}

	oUv = (uCurrentTextureMatrix * vec4(iUv, 1.0, 1.0)).xy;

	oColor = iColor;

	if (uIsLight)
	{
		vec4 result;
		if(uMaterial.isEmissive)
		{
			vec4 result = clamp(ambient + diffuse + specular + vec4(uMaterial.emission.x, uMaterial.emission.y, uMaterial.emission.z, 1.0), 0.0, 1.0);
			oColor *= result;
		}
		else
		{
			vec4 result = clamp(ambient + diffuse + specular, 0.0, 1.0);
			oColor *= result;
		}
		
		
	}
	else
	{
		vec4 globalAmbient = vec4(1.0);
		oColor *= globalAmbient * uMaterial.ambient;
	}
}
