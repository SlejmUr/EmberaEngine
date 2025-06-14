#version 460 core

layout (location = 0) out vec4 gNormal;
layout (location = 1) out vec4 gPosition;
layout (location = 2) out vec4 gFlagBuffer;

in vec3 Normal;
in vec3 Tangent;
in vec3 BiTangent;
in vec2 texCoords;
in vec3 FragPos;
in vec3 WorldPos;

uniform int HIGHLIGHT_BIT = 0;

void main() {
	gNormal = vec4(Normal, 1.0);
	gPosition = vec4(FragPos, 1.0);


	gFlagBuffer.r = 0x01u * HIGHLIGHT_BIT;
}