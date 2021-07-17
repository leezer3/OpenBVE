#version 150 core
precision highp int;

struct Light
{
	vec3 position;
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
	vec4 lightModel;
};

struct MaterialColor
{
	vec4 ambient;
	vec4 diffuse;
	vec4 specular;
	vec3 emission;
	float shininess;
};

in vec3 iPosition;
in vec3 iNormal;
in vec2 iUv;
in vec4 iColor;

uniform mat4 uCurrentProjectionMatrix;
uniform mat4 uCurrentModelViewMatrix;
uniform mat4 uCurrentTextureMatrix;

uniform bool uIsLight;
uniform Light uLight;
uniform MaterialColor uMaterial;
uniform int uMaterialFlags;

out vec4 oViewPos;
out vec2 oUv;
out vec4 oColor;
out vec4 oLightResult;

vec4 getLightResult()
{
	vec3 normal = normalize(mat3(transpose(inverse(uCurrentModelViewMatrix))) * vec3(iNormal.x, iNormal.y, -iNormal.z));
	float nDotVP = max(0.0, dot(normal, normalize(vec3(uLight.position))));
	float nDotHV = max(0.0, dot(normal, normalize(vec3(oViewPos.xyz + uLight.position))));
	float pf = nDotVP == 0.0 ? 0.0 : pow(nDotHV, uMaterial.shininess);

	vec4 ambient = vec4(uLight.ambient, 1.0);
	vec4 diffuse = vec4(uLight.diffuse, 1.0) * nDotVP;
	vec4 specular = vec4(uLight.specular, 1.0) * pf;

	vec4 sceneColor = (uMaterialFlags & 1) != 0 ? vec4(uMaterial.emission, 1.0) + uMaterial.ambient * uLight.lightModel : uLight.lightModel;
	vec4 finalColor = sceneColor + ambient * uMaterial.ambient + diffuse * uMaterial.specular + specular * uMaterial.specular;
	return clamp(finalColor, 0.0, 1.0);
}

void main()
{
	oViewPos = uCurrentModelViewMatrix * vec4(vec3(iPosition.x, iPosition.y, -iPosition.z), 1.0);
	gl_Position = uCurrentProjectionMatrix * oViewPos;

	oUv = (uCurrentTextureMatrix * vec4(iUv, 1.0, 1.0)).xy;
	oColor = iColor;
	oLightResult = uIsLight && (uMaterialFlags & 4) == 0 ? getLightResult() : uMaterial.ambient;
}
