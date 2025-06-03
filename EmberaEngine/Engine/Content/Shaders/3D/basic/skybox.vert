#version 440 core

layout (location = 0) in vec3 aPosition;
layout (location = 2) in vec2 aTexCoord;

uniform mat4 W_VIEW_MATRIX;
uniform mat4 W_PROJECTION_MATRIX;
uniform mat4 W_MODEL_MATRIX;

out vec3 texCoords;

void main()
{
    texCoords = aPosition;
    gl_Position = W_PROJECTION_MATRIX * W_VIEW_MATRIX * vec4(aPosition, 1.0);
}
