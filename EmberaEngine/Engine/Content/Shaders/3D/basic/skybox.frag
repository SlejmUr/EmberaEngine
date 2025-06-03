#version 440 core

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 EmissionColor;

in vec3 texCoords;

uniform samplerCube SKYBOX_TEXTURE;

void main()
{    
    FragColor = texture(SKYBOX_TEXTURE, texCoords);
    EmissionColor = vec4(0);
}