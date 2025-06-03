#version 440 core  

layout (location = 0) out vec4 FragColor;

in vec2 texCoords;

uniform int face;
uniform sampler2D panoramicTexture;

const float PI = 3.14159265359;

// Map cube face UVs to 3D direction
vec3 uvToDir(vec2 uv, int face) {
    vec3 dir;
    vec2 coord = uv * 2.0 - 1.0;

    if (face == 0)       dir = vec3(1.0, -coord.y, -coord.x);   // +X
    else if (face == 1)  dir = vec3(-1.0, -coord.y, coord.x);   // -X
    else if (face == 2)  dir = vec3(coord.x, 1.0, coord.y);      // +Y
    else if (face == 3)  dir = vec3(coord.x, -1.0, -coord.y);    // -Y
    else if (face == 4)  dir = vec3(coord.x, -coord.y, 1.0);     // +Z
    else                 dir = vec3(-coord.x, -coord.y, -1.0);   // -Z

    return normalize(dir);
}

// Convert 3D direction to equirectangular UV
vec2 dirToUV(vec3 dir) {
    float u = atan(dir.z, dir.x) / (2.0 * PI) + 0.5;
    float v = acos(clamp(dir.y, -1.0, 1.0)) / PI;
    return vec2(u, v);
}

// Sample panorama using direction vector
vec3 panoramaToCubemap(vec2 uv, int face) {
    vec3 dir = uvToDir(uv, face);
    vec2 st = dirToUV(dir);
    return textureLod(panoramicTexture, st, 0).rgb;
}

void main() {
    FragColor = vec4(panoramaToCubemap(texCoords, face), 1.0);
}
