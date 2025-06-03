#version 440 core

layout(location = 0) out vec4 FragColor;

in vec2 texCoords;

uniform sampler2D gDepth;
uniform sampler2D gPosition;

#define LIGHT_TYPE_MASK 0xC0000000u // top 2 bits
#define LIGHT_INDEX_MASK 0x3FFFFFFFu // lower 30 bits

#define TYPE_POINT 0u
#define TYPE_SPOT  1u
#define TYPE_DIRECTIONAL 2u

struct LightGrid {
	uint offset;
	uint count;
};

struct PointLight {
	vec4 position;
	vec4 color;
	float range;
};

layout (std430, binding = 0) buffer pointLightSSBO {
	PointLight pointLights[];
};

layout (std430, binding = 3) buffer lightGridSSBO {
	LightGrid lightGrids[];
};

layout(std430, binding = 6) buffer globalLightIndexListSSBO {
	uint globalLightIndexList[];
};

layout(std430, binding = 7) buffer screenViewData {
	mat4 inverseProjectionMatrix;
	uvec4 tileSizes;
	uint screenWidth;
	uint screenHeight;
	float sliceScaling;
	float sliceBias;
};

uniform vec3 C_VIEWPOS;
uniform float zNear, zFar;

float linearDepth(float depthSample) {
	float z = depthSample * 2.0 - 1.0;
	return (2.0 * zNear * zFar) / (zFar + zNear - z * (zFar - zNear));
}

vec3 reconstructWorldPos(vec2 uv, float depth) {
	vec4 clip = vec4(uv * 2.0 - 1.0, depth * 2.0 - 1.0, 1.0);
	vec4 view = inverseProjectionMatrix * clip;
	view /= view.w;
	return (view.xyz);
}

void main() {
	vec3 fragWorldPos = texture(gPosition, texCoords).xyz;
	vec3 viewVec = fragWorldPos - C_VIEWPOS;
	float linearZ = length(fragWorldPos - C_VIEWPOS);
	float depth = texture(gDepth, texCoords).z;

	float b = length(viewVec);
	float a = 0.0;
	float safeZ = max(linearZ, 0.0001);
	uint zTile = uint(clamp(log2(safeZ) * sliceScaling + sliceBias, 0.0, float(tileSizes.z - 1)));
	uvec2 tileXY = uvec2(gl_FragCoord.xy) / tileSizes.w;
	uvec3 tiles = uvec3(tileXY, zTile);

	uint tileIndex = tiles.x +
					 tileSizes.x * tiles.y +
					 tileSizes.x * tileSizes.y * tiles.z;



	vec3 fogColor = vec3(0.0);
	uint offset = lightGrids[tileIndex].offset;
	uint count  = lightGrids[tileIndex].count;

	for (uint i = 0; i < count; ++i) {
		uint lightVectorIndex = globalLightIndexList[offset + i];

		uint lightType  = (lightVectorIndex & LIGHT_TYPE_MASK) >> 30;
		uint lightIndex = (lightVectorIndex & LIGHT_INDEX_MASK);

		if (lightType == TYPE_POINT) {
			PointLight light = pointLights[lightIndex];
			vec3 L = light.position.xyz - C_VIEWPOS;
			float projLen = dot(normalize(viewVec), L);
			vec3 closest = C_VIEWPOS + normalize(viewVec) * projLen;
			float h = max(length(light.position.xyz - closest), 0.001);
			float scatter = (atan(b / h) - atan(a / h)) / h;

			float fade = clamp(1.0 - length(fragWorldPos - light.position.xyz) / light.range, 0.0, 1.0);
			fogColor += light.color.rgb * light.color.a * scatter * fade;
		}
	}

	FragColor = vec4(fogColor, 1);
}
