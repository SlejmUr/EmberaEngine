#version 440 core

layout (location = 0) out float FragColor;

in vec2 texCoords;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D texNoise;
uniform sampler2D gDepth;

uniform vec3 samples[64]; //change this when increasing or decreasing samples;

int kernelSize = 64; // change this too
float radius = 5;
float bias = 0.0025;

uniform mat4 W_PROJECTION_MATRIX;
uniform mat4 W_INVERSE_VIEW_MATRIX;
uniform mat4 W_VIEW_MATRIX;
uniform vec2 screenDimensions;

void main() {
	vec2 noiseScale = vec2(screenDimensions.x / 4.0, screenDimensions.y / 4.0);

	vec3 FragPos = texture(gPosition, texCoords).xyz;
	vec3 fragWorldPos = (W_INVERSE_VIEW_MATRIX * vec4(FragPos, 1.0)).xyz;

	vec3 N = normalize(texture(gNormal, texCoords)).rgb;
	vec3 randomVec = normalize(texture(texNoise, texCoords * noiseScale).xyz);


	vec3 tangent = normalize(randomVec - N * dot(randomVec, N));
	vec3 bitangent = cross(N, tangent);

	mat3 TBN = mat3(tangent, bitangent, N);

	float occlusion = 0.0;
	for (int i = 0; i < kernelSize; ++i) {
		vec3 samplePos = TBN * samples[i].xyz;
		samplePos = fragWorldPos + samplePos * radius;
		samplePos = (W_VIEW_MATRIX * vec4(samplePos, 1.0)).xyz;

		vec4 offset = vec4(samplePos, 1.0);
		offset = W_PROJECTION_MATRIX * offset;
		offset.xyz /= offset.w;
		offset.xyz = offset.xyz * 0.5 + 0.5;

		float sampleDepth = texture(gPosition, offset.xy).z;

		float rangeCheck = smoothstep(0.0, 1.0, radius / abs(FragPos.z - sampleDepth));
		occlusion += (sampleDepth >= samplePos.z + bias ? 1.0 : 0.0) * rangeCheck;
	}
	occlusion = 1.0 - (occlusion / kernelSize);

	FragColor = occlusion;
}