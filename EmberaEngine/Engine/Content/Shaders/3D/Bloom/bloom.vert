#version 440 core

out vec2 texCoord;

void main()
{
    texCoord = vec2((gl_VertexID << 1) & 2, gl_VertexID & 2);

    gl_Position = vec4(texCoord * vec2(2.0f, -2.0f) + vec2(-1.0f, 1.0f), 0.0f, 1.0f);
    
    texCoord.y = 1 - texCoord.y;
}
