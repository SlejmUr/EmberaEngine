#version 440 core

const float PI = 3.14159265359;
const float GAMMA = 2.2;

layout (location = 0) out vec4 FragColor;

in vec2 texCoords;
in vec3 Normal;
in vec3 Tangent;
in vec3 BiTangent;
in vec3 FragPos;
in vec3 WorldPos;

uniform vec4 COLOR;
uniform vec3 C_VIEWPOS;

vec3 colors[8] = vec3[](
   vec3(0, 0, 0),    vec3( 0,  0,  1), vec3( 0, 1, 0),  vec3(0, 1,  1),
   vec3(1,  0,  0),  vec3( 1,  0,  1), vec3( 1, 1, 0),  vec3(1, 1, 1)
);

struct LightGrid {
	uint offset;
	uint count;
};

struct PointLight {
	vec4 position;
    vec4 color;
    bool enabled;
    float intensity;
    float range;
};

struct Material {
    sampler2D ALBEDO_TEX;
    sampler2D ROUGHNESS_TEX;
    sampler2D EMISSION_TEX;
    sampler2D NORMAL_TEX;
    vec3  albedo;
    vec3 emission;
    float emissionStr;
    float metallic;
    float roughness;
    float ao;
};

layout (std430, binding = 0) buffer pointLightSSBO {
    PointLight pointLights[];
};

layout (std430, binding = 1) buffer lightGridSSBO {
    LightGrid lightGrids[];
};

layout(std430, binding = 4) buffer globalLightIndexListSSBO {
    uint globalLightIndexList[];
};

layout (std430, binding = 5) buffer screenViewData {
    mat4 inverseProjectionMatrix;
    uvec4 tileSizes;
    uint screenWidth;
    uint screenHeight;
    float sliceScaling;
    float sliceBias;
};

uniform Material material;
uniform float zNear;
uniform float zFar;

float DistributionGGX(vec3 N, vec3 H, float roughness);
float GeometrySchlickGGX(float NdotV, float roughness);
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness);
vec3 fresnelSchlick(float cosTheta, vec3 F0);
vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness);
vec3 CalcPointLight(uint index, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 albedo, float rough, float metal, vec3 F0,  float viewDistance);


float linearDepth(float depthSample);

void main() {

    vec3 N = normalize(Normal);
    vec3 V = normalize(C_VIEWPOS - WorldPos);

    vec3 albedo = material.albedo;
    float alpha = COLOR.a;

    
    vec3 viewDirection = normalize(C_VIEWPOS - FragPos);
    vec3 R = reflect(-viewDirection, N);

    vec3 F0 = vec3(0.04);
    F0 = mix(F0, albedo, material.metallic);


    uint zTile     = uint(max(log2(linearDepth(gl_FragCoord.z)) * sliceScaling + sliceBias, 0.0));
    uvec3 tiles    = uvec3( uvec2( gl_FragCoord.xy / tileSizes[3] ), zTile);
    uint tileIndex = tiles.x +
                     tileSizes.x * tiles.y +
                     (tileSizes.x * tileSizes.y) * tiles.z;  

    vec3 radianceOut = vec3(0.0);

    float viewDistance = length(C_VIEWPOS - FragPos);

    // Point Lights
    uint lightCount = lightGrids[tileIndex].count;
    uint lightIndexOffset = lightGrids[tileIndex].offset;

    radianceOut = vec3(0.01);

    for (uint i = 0; i < lightCount; i++) {
        uint lightVectorIndex = globalLightIndexList[lightIndexOffset + i];
        radianceOut += CalcPointLight(lightVectorIndex, N, FragPos, viewDirection, albedo, material.roughness, material.metallic, F0, viewDistance);
    }

    uint clusterIndex = tiles.x + tiles.y * tileSizes.x + tileIndex * tileSizes.x * tileSizes.y;
    FragColor = vec4(radianceOut, 1.0);
    //FragColor = vec4(colors[uint(mod(float(zTile), 8.0))], 1.0);
    //FragColor = vec4(FragPos.x, FragPos.y, 0, 1);
}

vec3 CalcPointLight(uint index, vec3 normal, vec3 fragPos,
                    vec3 viewDir, vec3 albedo, float rough,
                    float metal, vec3 F0,  float viewDistance){
    //Point light basics
    vec3 position = pointLights[index].position.xyz;
    vec3 color    = 100.0 * pointLights[index].color.rgb;
    float radius  = pointLights[index].range;

    //Stuff common to the BRDF subfunctions 
    vec3 lightDir = normalize(position - fragPos);
    vec3 halfway  = normalize(lightDir + viewDir);
    float nDotV = max(dot(normal, viewDir), 0.0);
    float nDotL = max(dot(normal, lightDir), 0.0);

    //Attenuation calculation that is applied to all
    float distance    = length(position - fragPos);
    float attenuation = pow(clamp(1 - pow((distance / radius), 4.0), 0.0, 1.0), 2.0)/(1.0  + (distance * distance) );
    vec3 radianceIn   = color * attenuation;

    //Cook-Torrance BRDF
    float NDF = DistributionGGX(normal, halfway, rough);
    float G   = GeometrySmith(normal, viewDir, lightDir, rough);
    vec3  F   = fresnelSchlick(max(dot(halfway,viewDir), 0.0), F0);

    //Finding specular and diffuse component
    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - metal;

    vec3 numerator = NDF * G * F;
    float denominator = 4.0 * nDotV * nDotL;
    vec3 specular = numerator / max(denominator, 0.0000001);
    // vec3 specular = numerator / denominator;

    vec3 radiance = (kD * (albedo / PI) + specular ) * radianceIn * nDotL;
    // //shadow stuff
    // vec3 fragToLight = fragPos - position;
    // float shadow = calcPointLightShadows(depthMaps[index], fragToLight, viewDistance);
    
    // radiance *= (1.0 - shadow);

    return radiance;
}

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a = roughness*roughness;
    float a2 = a*a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;

    float nom   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}
// ----------------------------------------------------------------------------
vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

// ----------------------------------------------------------------------------
vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}


float linearDepth(float depthSample){
    float depthRange = 2.0 * depthSample - 1.0;
    // Near... Far... wherever you are...
    float linear = 2.0 * zNear * zFar / (zFar + zNear - depthRange * (zFar - zNear));
    return linear;
}