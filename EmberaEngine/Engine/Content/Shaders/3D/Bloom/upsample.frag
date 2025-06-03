#version 330 core

layout (location = 0) out vec3 upsample;

uniform sampler2D INPUT_TEXTURE;
uniform float filterRadius;

in vec2 texCoord;

void main()
{
	float x = filterRadius;
	float y = filterRadius;

	vec3 a = texture(INPUT_TEXTURE, vec2(texCoord.x - x, texCoord.y + y)).rgb;
	vec3 b = texture(INPUT_TEXTURE, vec2(texCoord.x,     texCoord.y + y)).rgb;
	vec3 c = texture(INPUT_TEXTURE, vec2(texCoord.x + x, texCoord.y + y)).rgb;

	vec3 d = texture(INPUT_TEXTURE, vec2(texCoord.x - x, texCoord.y)).rgb;
	vec3 e = texture(INPUT_TEXTURE, vec2(texCoord.x,     texCoord.y)).rgb;
	vec3 f = texture(INPUT_TEXTURE, vec2(texCoord.x + x, texCoord.y)).rgb;

	vec3 g = texture(INPUT_TEXTURE, vec2(texCoord.x - x, texCoord.y - y)).rgb;
	vec3 h = texture(INPUT_TEXTURE, vec2(texCoord.x,     texCoord.y - y)).rgb;
	vec3 i = texture(INPUT_TEXTURE, vec2(texCoord.x + x, texCoord.y - y)).rgb;

	upsample = e*4.0;
	upsample += (b+d+f+h)*2.0;
	upsample += (a+c+g+i);
	upsample *= 1.0 / 16.0;
}