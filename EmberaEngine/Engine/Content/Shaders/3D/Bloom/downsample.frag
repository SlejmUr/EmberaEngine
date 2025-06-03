#version 330 core

layout (location = 0) out vec3 downsample;

uniform sampler2D INPUT_TEXTURE;
uniform vec2 srcResolution;
uniform int mipLevel = 1;

in vec2 texCoord;

vec3 PowVec3(vec3 v, float p)
{
    return vec3(pow(v.x, p), pow(v.y, p), pow(v.z, p));
}

const float invGamma = 1.0 / 2.2;
vec3 ToSRGB(vec3 v)   { return PowVec3(v, invGamma); }

float sRGBToLuma(vec3 col)
{
	return dot(col, vec3(0.299f, 0.587f, 0.114f));
}

float KarisAverage(vec3 col)
{
	float luma = sRGBToLuma(ToSRGB(col)) * 0.25f;
	return 1.0f / (1.0f + luma);
}

void main()
{

	vec2 srcTexelSize = 1.0 / srcResolution;
	float x = srcTexelSize.x;
	float y = srcTexelSize.y;

	vec3 a = textureLod(INPUT_TEXTURE, vec2(texCoord.x - 2*x, texCoord.y + 2*y), 0).rgb;
	vec3 b = textureLod(INPUT_TEXTURE, vec2(texCoord.x,       texCoord.y + 2*y), 0).rgb;
	vec3 c = textureLod(INPUT_TEXTURE, vec2(texCoord.x + 2*x, texCoord.y + 2*y), 0).rgb;

	vec3 d = textureLod(INPUT_TEXTURE, vec2(texCoord.x - 2*x, texCoord.y), 0).rgb;
	vec3 e = textureLod(INPUT_TEXTURE, vec2(texCoord.x,       texCoord.y), 0).rgb;
	vec3 f = textureLod(INPUT_TEXTURE, vec2(texCoord.x + 2*x, texCoord.y), 0).rgb;

	vec3 g = textureLod(INPUT_TEXTURE, vec2(texCoord.x - 2*x, texCoord.y - 2*y), 0).rgb;
	vec3 h = textureLod(INPUT_TEXTURE, vec2(texCoord.x,       texCoord.y - 2*y), 0).rgb;
	vec3 i = textureLod(INPUT_TEXTURE, vec2(texCoord.x + 2*x, texCoord.y - 2*y), 0).rgb;

	vec3 j = textureLod(INPUT_TEXTURE, vec2(texCoord.x - x, texCoord.y + y), 0).rgb;
	vec3 k = textureLod(INPUT_TEXTURE, vec2(texCoord.x + x, texCoord.y + y), 0).rgb;
	vec3 l = textureLod(INPUT_TEXTURE, vec2(texCoord.x - x, texCoord.y - y), 0).rgb;
	vec3 m = textureLod(INPUT_TEXTURE, vec2(texCoord.x + x, texCoord.y - y), 0).rgb;

	vec3 groups[5];
	switch (mipLevel)
	{
		case 0:
		  groups[0] = (a+b+d+e) * (0.125f/4.0f);
		  groups[1] = (b+c+e+f) * (0.125f/4.0f);
		  groups[2] = (d+e+g+h) * (0.125f/4.0f);
		  groups[3] = (e+f+h+i) * (0.125f/4.0f);
		  groups[4] = (j+k+l+m) * (0.5f/4.0f);
		  groups[0] *= KarisAverage(groups[0]);
		  groups[1] *= KarisAverage(groups[1]);
		  groups[2] *= KarisAverage(groups[2]);
		  groups[3] *= KarisAverage(groups[3]);
		  groups[4] *= KarisAverage(groups[4]);
		  downsample = groups[0]+groups[1]+groups[2]+groups[3]+groups[4];
		  downsample = max(downsample, 0.0001f);
		  break;
		default:
		  downsample = e*0.125;                // ok
		  downsample += (a+c+g+i)*0.03125;     // ok
		  downsample += (b+d+f+h)*0.0625;      // ok
		  downsample += (j+k+l+m)*0.125;       // ok
		  break;
	}
}