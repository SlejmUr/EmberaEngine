#version 440 core

in vec2 texCoords;

const float GAMMA = 2.2;

layout (location = 0) out vec4 FragColor;

uniform sampler2D SCREEN_TEXTURE;
uniform sampler2D AO_TEXTURE;
uniform sampler2D BLOOM_TEXTURE;
uniform sampler2D VOLUMETRIC_TEXTURE;

uniform int TONEMAP_FUNCTION;
uniform int USE_AO;
uniform int USE_BLOOM;

uniform float EXPOSURE = 0.05;

float luminance(vec3 v) {
    return dot(v, vec3(0.2126f, 0.7152f, 0.0722f));
}

vec3 change_luminance(vec3 c_in, float l_out) {
    float l_in = luminance(c_in);
    return c_in * (l_out / max(l_in, 1e-4));
}

vec3 tonemapACES(vec3 c) {
    float a = 2.51f;
    float b = 0.03f;
    float y = 2.43f;
    float d = 0.59f;
    float e = 0.14f;
    return clamp((c * (a * c + b)) / (c * (y * c + d) + e), 0.0, 1.0);
}

vec3 filmic(vec3 x) {
    vec3 X = max(vec3(0.0), x - 0.004);
    vec3 result = (X * (6.2 * X + 0.5)) / (X * (6.2 * X + 1.7) + 0.06);
    return pow(result, vec3(1.0)); // Gamma correction comes later
}

vec3 reinhard(vec3 v) {
    return v / (1.0f + v);
}

void main() {
    vec4 tex = texture(SCREEN_TEXTURE, texCoords);

    float ao = USE_AO != 0 ? texture(AO_TEXTURE, texCoords).r : 1.0;
    vec3 bloom = USE_BLOOM != 0 ? texture(BLOOM_TEXTURE, texCoords).rgb : vec3(0.0);
    vec3 volumetric = texture(VOLUMETRIC_TEXTURE, texCoords).rgb;

    tex.rgb *= ao;
    tex.rgb += bloom;
    //tex.rgb += volumetric;


    tex.rgb *= EXPOSURE;

    if (TONEMAP_FUNCTION == 0) {
        tex.rgb = tonemapACES(tex.rgb);
    } else if (TONEMAP_FUNCTION == 1) {
        tex.rgb = filmic(tex.rgb);
    } else if (TONEMAP_FUNCTION == 2) {
        tex.rgb = reinhard(tex.rgb);
    }

    tex.rgb = pow(tex.rgb, vec3(1.0/GAMMA)); // gamma correction AFTER

    
    FragColor = tex;
}
