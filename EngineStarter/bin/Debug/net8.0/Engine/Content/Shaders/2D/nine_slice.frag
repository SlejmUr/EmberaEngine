#version 330 core

layout(location = 0) out vec4 color;

in vec2 v_TexCoord;

uniform sampler2D u_Texture;
uniform vec4 u_Color = vec4(0,0,0,0);

uniform vec2 u_dimensions;

// Later before other operations
uniform vec4 borders = vec4(0);

#define B vec4(50., 20., 30., 20.)


float map(float value, float originalMin, float originalMax, float newMin, float newMax) {
    return (value - originalMin) / (originalMax - originalMin) * (newMax - newMin) + newMin;
} 

float processAxis(float coord, float textureBorder, float windowBorder) {
    if (coord < windowBorder)
        return map(coord, 0, windowBorder, 0, textureBorder) ;
    if (coord < 1 - windowBorder) 
        return map(coord,  windowBorder, 1 - windowBorder, textureBorder, 1 - textureBorder);
    return map(coord, 1 - windowBorder, 1, 1 - textureBorder, 1);
} 

vec2 uv9slice(vec2 uv, vec2 s, vec4 b)
{
    vec2 t = clamp((s * uv - b.xy) / (s - b.xy - b.zw), 0., 1.);
	return mix(uv * s, 1. - s * (1. - uv), t);
}


void main()
{

    vec2 ts = vec2(textureSize(u_Texture, 0));

    vec2 s = u_dimensions / ts;

    vec4 b = min(borders / ts.xyxy, vec4(.499));

    vec2 newUV = uv9slice(v_TexCoord, s, b);

    vec4 tex = texture(u_Texture, newUV);
	color = tex + u_Color;
    //color = vec4(newUV.x, newUV.y, 1, 1);
}