#version 440 core
out vec4 FragColor;

flat in vec3 color;

void main()
{
    FragColor = vec4(color, 1.0);
}
