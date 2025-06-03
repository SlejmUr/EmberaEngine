#version 440 core

layout(location = 0) out vec4 FragColor;
layout(location = 1) out vec4 EmissionColor;

in vec2 v_TexCoord;

uniform sampler2D INPUT_TEXTURE;
uniform vec4 u_Color = vec4(0,0,0,0);

void main()
{
    vec4 tex = texture(INPUT_TEXTURE, v_TexCoord);
	FragColor = tex + u_Color;
}