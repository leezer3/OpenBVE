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

uniform mat4 uCurrentTranslateMatrix;
uniform mat4 uCurrentScaleMatrix;
uniform mat4 uCurrentRotateMatrix;
uniform mat4 uCurrentTextureTranslateMatrix;
uniform mat4 uCurrentProjectionMatrix;
uniform mat4 uCurrentViewMatrix;

uniform vec3 uEyePosition;
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
	vec3 worldPos = (uCurrentTranslateMatrix * uCurrentRotateMatrix * uCurrentScaleMatrix * vec4(iPosition, 1.0)).xyz;
	vec4 viewPos = uCurrentViewMatrix * vec4(worldPos, 1.0);
	gl_Position = uCurrentProjectionMatrix * viewPos;

	// Lighting
	// Ambient
	vec4 ambient  = uLight.ambient * uMaterial.ambient;

	// Diffuse
	vec3 norm = normalize(iNormal);
	vec3 lightDir = normalize(uLight.position - worldPos);
	float diff = max(dot(norm, lightDir), 0.0);
	vec4 diffuse = uLight.diffuse * (diff * uMaterial.diffuse);

	// Specular
	vec3 eyeDir = normalize(uEyePosition - worldPos);
	vec3 reflectDir = reflect(-lightDir, norm);
	float spec = pow(max(dot(eyeDir, reflectDir), 0.0), uMaterial.shininess);
	vec4 specular = uLight.specular * (spec * uMaterial.specular);

	vec4 result = ambient + diffuse + specular + uMaterial.emission;

	// Fog
	oFogFactor = 1.0;

	if (uIsFog)
	{
		oFogFactor *= clamp((uFogEnd - length(viewPos)) / (uFogEnd - uFogStart), 0.0, 1.0);
	}

	oUv = (uCurrentTextureTranslateMatrix * vec4(iUv, 1.0, 1.0)).xy;

	oColor = iColor;

	if (uIsLight)
	{
		oColor *= result;
	}
}
