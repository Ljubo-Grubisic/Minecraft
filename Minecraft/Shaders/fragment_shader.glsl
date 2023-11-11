#version 420 core
struct DirLight{
	vec3 direction;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};


in vec3 Normal;
in vec3 FragPos;
in vec2 TexCoords;

out vec4 outputColor;

uniform sampler2D texture_diffuse1;
uniform DirLight dirLight;
uniform vec3 viewPos; 

vec3 CalculateDirLight(DirLight light, sampler2D texture_diffuse, vec3 normal){
	// Ambient
	vec3 ambient = texture(texture_diffuse, TexCoords).rgb * light.ambient;

	// Diffuse
	vec3 lightDir = normalize(-light.direction);

	float diff = max(dot(normal, lightDir), 0.0);
	vec3 diffuse = (diff * texture(texture_diffuse, TexCoords).rgb) * light.diffuse;
	
	return (ambient + diffuse);
};

void main()
{
	outputColor = vec4(CalculateDirLight(dirLight, texture_diffuse1, Normal), 1.0);
}