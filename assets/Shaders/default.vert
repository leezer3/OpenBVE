#version 150
precision highp int;
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
	int flags; //bitmask as per stored in the object
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

out vec4 oViewPos;
out vec2 oUv;
out vec4 oColor;
out vec4 lightResult;


//Temp working colors
vec4 Ambient;
vec4 Diffuse;
vec4 Specular;

void findDirectionalLight(in vec3 normal)
{
   float pf;
   float nDotVP = max(0.0, dot(normal, normalize(vec3 (uLight.position))));
   float nDotHV = max(0.0, dot(normal, normalize(vec3(oViewPos.xyz + uLight.position))));

   if (nDotVP == 0.0)
   {
       pf = 0.0;
   }
   else
   {
       pf = pow(nDotHV, uMaterial.shininess);
   }
   
   if((uMaterial.flags & 1) != 0)
   {
	  //Emissive, so replace all lights with pure white
      Ambient  = vec4(1.0);
	  Diffuse  = vec4(1.0);
	  Specular = vec4(1.0);
   }
   else
   {
      Ambient  += vec4(uLight.ambient, 1.0);
      Diffuse  += vec4(uLight.diffuse, 1.0) * nDotVP;
	  Specular += vec4(uLight.specular, 1.0) * pf;
   }
   
}

vec4 getLightResult(in vec3 normal, in vec4 ecPosition)
{
    vec4 color;
    vec3 ecPosition3;
    vec3 eye;

    ecPosition3 = (vec3 (ecPosition)) / ecPosition.w;
    eye = vec3 (0.0, 0.0, 1.0);

    Ambient  = vec4 (0.0);
    Diffuse  = vec4 (0.0);
    Specular = vec4 (0.0);

    findDirectionalLight(normal);

    color = vec4(0.0, 0.0, 0.0, 1.0) + Ambient  * uMaterial.ambient + Diffuse  * uMaterial.specular;
    color += Specular * uMaterial.specular;
    color = clamp(color, 0.0, 1.0);
	return color;
}


void main()
{
	oViewPos = uCurrentModelViewMatrix * vec4(vec3(iPosition.x, iPosition.y, -iPosition.z), 1.0);
	gl_Position = uCurrentProjectionMatrix * oViewPos;

	vec3 eyeNormal = normalize(mat3(transpose(inverse(uCurrentModelViewMatrix))) * vec3(iNormal.x, iNormal.y, -iNormal.z));
	vec3 eyePosition = vec3(uCurrentModelViewMatrix * vec4(iPosition.x, iPosition.y, -iPosition.z, 1.0));

	oUv = (uCurrentTextureMatrix * vec4(iUv, 1.0, 1.0)).xy;

	oColor = iColor;
	
	if (uIsLight)
	{
		lightResult = getLightResult(eyeNormal, oViewPos);

		if((uMaterial.flags & 1) != 0)
		{
			lightResult = clamp(lightResult + vec4(uMaterial.emission, 1.0), 0.0, 1.0);
		}
	}
	else
	{
		lightResult = uMaterial.ambient;
	}
	oColor.rgb *= oColor.a;
}
