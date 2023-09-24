#version 420 core
in vec3 Normal;
in vec3 FragPos;
in vec2 TexCoords;

out vec4 outputColor;

uniform sampler2D texture_diffuse1;

void main()
{
	outputColor = texture(texture_diffuse1, TexCoords);
}