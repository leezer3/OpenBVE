#version 130

struct Light
{
    vec3 position;
    vec4 ambient;
    vec4 diffuse;
    vec4 specular;
};

struct MaterialColor
{
    vec4 ambient;
    vec4 diffuse;
    vec4 specular;
	vec4 emission;
    float shininess;
}; 

in vec3 iPosition;
in vec3 iNormal;
in vec2 iUv;
in vec4 iColor;

uniform mat4 uCurrentProjectionMatrix;
uniform mat4 uCurrentModelViewMatrix;
uniform mat4 uCurrentNormalMatrix;
uniform mat4 uCurrentTextureMatrix;

uniform bool uIsLight;
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
	vec4 viewPos = uCurrentModelViewMatrix * vec4(iPosition, 1.0);
	vec4 viewNormal = uCurrentNormalMatrix * vec4(iNormal, 1.0);
	gl_Position = uCurrentProjectionMatrix * viewPos;

	// Lighting
	// Ambient
	vec4 ambient  = uLight.ambient * uMaterial.ambient;

	// Diffuse
	vec3 norm = normalize(viewNormal.xyz);
	vec3 eye = normalize(-viewPos.xyz);
	float diff = max(dot(norm, uLight.position), 0.0);
	vec4 diffuse = uLight.diffuse * (diff * uMaterial.diffuse);

	// Specular
	vec4 specular = vec4(0.0);

	if(diff > 0.0)
	{
		vec3 halfVector = normalize(uLight.position + eye);
		float spec = pow(max(dot(halfVector, norm), 0.0), uMaterial.shininess);
		specular = uLight.specular * (spec * uMaterial.specular);
	}

	vec4 result = clamp(ambient + diffuse + specular + uMaterial.emission, 0.0, 1.0);

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
		oColor *= result;
	}
	else
	{
		vec4 globalAmbient = vec4(1.0);
		oColor *= globalAmbient * uMaterial.ambient;
	}
}
