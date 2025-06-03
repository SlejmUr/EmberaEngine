#version 440 core

layout (location = 0) out vec4 gNormal;
layout (location = 1) out vec4 gPosition;

in vec3 Normal;
in vec3 Tangent;
in vec3 BiTangent;
in vec2 texCoords;
in vec3 FragPos;
in vec3 WorldPos;


void main() {
	gNormal = vec4(Normal, 1.0);
	gPosition = vec4(FragPos, 1.0);
}