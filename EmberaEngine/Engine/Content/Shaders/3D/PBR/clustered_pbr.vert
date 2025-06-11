#version 440 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBiTangent;

uniform mat4 W_VIEW_MATRIX;
uniform mat4 W_MODEL_MATRIX;
uniform mat4 W_PROJECTION_MATRIX;

out vec3 Normal;
out vec4 Tangent; // xyz = tangent, w = handedness
out vec3 BiTangent;
out vec2 texCoords;
out vec3 FragPos;
out vec3 WorldPos;

void main()
{
    mat3 normalMatrix = transpose(inverse(mat3(W_MODEL_MATRIX)));

    vec3 N = normalize(normalMatrix * aNormal);
    vec3 T = normalize(normalMatrix * aTangent);
    vec3 B = normalize(normalMatrix * aBiTangent);

    // Compute handedness (+1 or -1)
    float handedness = (dot(cross(N, T), B) < 0.0) ? -1.0 : 1.0;

    Normal = N;
    Tangent = vec4(T, handedness);
    BiTangent = B;

    vec4 worldPosition = W_MODEL_MATRIX * vec4(aPosition, 1.0);
    WorldPos = vec3(worldPosition);
    FragPos = vec3(worldPosition);
    texCoords = aTexCoord;

    gl_Position = W_PROJECTION_MATRIX * W_VIEW_MATRIX * worldPosition;
}
