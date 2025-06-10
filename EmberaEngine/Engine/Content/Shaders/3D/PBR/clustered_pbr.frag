#version 440 core

const float PI = 3.14159265359;

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 EmissionColor;
layout(early_fragment_tests) in;

in vec2 texCoords;
in vec3 Normal;
in vec3 Tangent;
in vec3 BiTangent;
in vec3 FragPos;
in vec3 WorldPos;

const vec3 colors[8] = vec3[](
   vec3(0, 0, 0),    vec3( 0,  0,  1), vec3( 0, 1, 0),  vec3(0, 1,  1),
   vec3(1,  0,  0),  vec3( 1,  0,  1), vec3( 1, 1, 0),  vec3(1, 1, 1)
);

struct LightGrid {
	uint offset;
	uint count;
};

#define LIGHT_TYPE_MASK 0xC0000000u // top 2 bits
#define LIGHT_INDEX_MASK 0x3FFFFFFFu // lower 30 bits

#define TYPE_POINT 0u
#define TYPE_SPOT  1u
#define TYPE_DIRECTIONAL 2u

struct DirectionalLight {
	vec4 direction; // xyz direction w enabled/disabled
	vec4 color; // xyz color w intensity
};

struct PointLight {
	vec4 position;
	vec4 color;
	float range;
};

struct SpotLight {
	vec4 position;     // xyz = position, w = enabled
    vec4 color;        // rgb = color, w = intensity
    vec4 direction;    // xyz = direction, w = range
    float innerCutoff; // degrees
    float outerCutoff; // degrees
};

struct Material {
	sampler2D DIFFUSE_TEX;
	sampler2D NORMAL_TEX;
	sampler2D ROUGHNESS_TEX;
	sampler2D EMISSION_TEX;
	vec4  albedo;
	vec3 emission;
	float emissionStr;
	float metallic;
	float roughness;
	int useDiffuseMap;
	int useNormalMap;
	int useRoughnessMap;
	int useEmissionMap;
	bool useIBL;
};

layout (std430, binding = 0) buffer pointLightSSBO {
	PointLight pointLights[];
};

layout (std430, binding = 1) buffer spotLightSSBO {
	SpotLight spotLights[];
};

layout (std430, binding = 2) buffer directionalLightSSBO {
	DirectionalLight directionalLight;
};

layout (std430, binding = 3) buffer lightGridSSBO {
	LightGrid lightGrids[];
};

layout(std430, binding = 6) buffer globalLightIndexListSSBO {
	uint globalLightIndexList[];
};

layout (std430, binding = 7) buffer screenViewData {
	mat4 inverseProjectionMatrix;
	uvec4 tileSizes;
	uint screenWidth;
	uint screenHeight;
	float sliceScaling;
	float sliceBias;
};

uniform Material material;
uniform vec3 C_VIEWPOS;
uniform float zNear;
uniform float zFar;

uniform vec3 ambientColor;
uniform float ambientFactor;


uniform bool useIBL;
uniform samplerCube irradianceMap;
uniform samplerCube prefilterMap;
uniform sampler2D   brdfLUT;  


float DistributionGGX(vec3 N, vec3 H, float roughness);
float GeometrySchlickGGX(float NdotV, float roughness);
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness);
vec3 fresnelSchlick(float cosTheta, vec3 F0);
vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness);
vec3 CalcDirectionalLight(vec3 normal, vec3 fragPos, vec3 viewDir, vec3 albedo, float rough, float metal, vec3 F0);
vec3 CalcPointLight(uint index, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 albedo, float rough, float metal, vec3 F0,  float viewDistance);
vec3 CalcSpotLight(uint index, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 albedo, float rough, float metal, vec3 F0, float viewDistance);
vec3 CalcIBL(vec3 N, vec3 V, vec3 F0, vec3 albedo, vec3 R, float roughness, float metallic);

float linearDepth(float depthSample);
vec3 GammaCorrect(vec3 value);
vec4 GetDiffuse();
float GetRoughness();
float GetMetallic();
vec3 GetNormal(vec3 N);
vec3 GetEmission();


void main() {

	vec3 N = GetNormal(normalize(Normal));
	vec3 V = normalize(C_VIEWPOS - WorldPos);

	vec4 diffuseColor = GetDiffuse();
//
//	if (diffuseColor.a == 0) discard;
//

	//diffuseColor.xyz *= (texture(material.AO_TEX, viewportCoords).r);

	vec3 albedo = diffuseColor.xyz;
	float alpha = diffuseColor.a;

	float roughnessValue = GetRoughness();

	
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

	uint lightCount = lightGrids[tileIndex].count;
	uint lightIndexOffset = lightGrids[tileIndex].offset;

	radianceOut = vec3(ambientColor) * ambientFactor;

	radianceOut += CalcDirectionalLight(N, FragPos, viewDirection, albedo, roughnessValue, material.metallic, F0);

	for (uint i = 0; i < lightCount; i++) {
		uint lightVectorIndex = globalLightIndexList[lightIndexOffset + i];

		uint lightType  = (lightVectorIndex & LIGHT_TYPE_MASK) >> 30;
		uint lightIndex = (lightVectorIndex & LIGHT_INDEX_MASK);

		if (lightType == TYPE_POINT) {
			radianceOut += CalcPointLight(lightIndex, N, FragPos, viewDirection, albedo, roughnessValue, material.metallic, F0, viewDistance);
		} else {
			radianceOut += CalcSpotLight(lightIndex, N, FragPos, viewDirection, albedo, roughnessValue, material.metallic, F0, viewDistance);
		}
	}

	if (useIBL) {
		radianceOut += CalcIBL(N, V, F0, albedo, R, material.roughness, material.metallic) * ambientFactor;
	}


	vec3 emissive = GetEmission() * material.emissionStr;
	radianceOut += emissive;

	FragColor = vec4(radianceOut, 1.0);
	EmissionColor = vec4(emissive, 1);
}

vec3 CalcIBL(vec3 N, vec3 V, vec3 F0, vec3 albedo, vec3 R, float roughness, float metallic) {
    vec3 F = fresnelSchlickRoughness(max(dot(N, V), 0.0), F0, roughness);
    
    vec3 kS = F;
    vec3 kD = 1.0 - kS;
    kD *= 1.0 - metallic;	  
    
    vec3 irradiance = texture(irradianceMap, N).rgb;
    vec3 diffuse      = irradiance * albedo;
    
    // sample both the pre-filter map and the BRDF lut and combine them together as per the Split-Sum approximation to get the IBL specular part.
    const float MAX_REFLECTION_LOD = 4.0;
    vec3 prefilteredColor = textureLod(prefilterMap, R,  roughness * MAX_REFLECTION_LOD).rgb;    
    vec2 brdf  = texture(brdfLUT, vec2(max(dot(N, V), 0.0), roughness)).rg;
    vec3 specular = prefilteredColor * (F * brdf.x + brdf.y);

	return (kD * diffuse + specular * 0.4);
}

vec3 CalcDirectionalLight(vec3 normal, vec3 fragPos,
						  vec3 viewDir, vec3 albedo, float rough,
						  float metal, vec3 F0) {

	if (directionalLight.direction.w == 0.0) { return vec3(0.0); }

	// Directional light basics
	vec3 lightDir = normalize(-directionalLight.direction.xyz); // Assuming lightDir points *toward* the surface
	vec3 color    = directionalLight.color.rgb * directionalLight.color.w;

	// Common BRDF terms
	vec3 halfway = normalize(lightDir + viewDir);
	float nDotV = max(dot(normal, viewDir), 0.0);
	float nDotL = max(dot(normal, lightDir), 0.0);
	vec3 radianceIn = color; // No attenuation for directional light

	// Cook-Torrance BRDF
	float NDF = DistributionGGX(normal, halfway, rough);
	float G   = GeometrySmith(normal, viewDir, lightDir, rough);
	vec3  F   = fresnelSchlick(max(dot(halfway, viewDir), 0.0), F0);

	// Finding specular and diffuse component
	vec3 kS = F;
	vec3 kD = vec3(1.0) - kS;
	kD *= 1.0 - metal;

	vec3 numerator = NDF * G * F;
	float denominator = 4.0 * nDotV * nDotL;
	vec3 specular = numerator / max(denominator, 0.0000001);

	vec3 radiance = (kD * (albedo / PI) + specular) * radianceIn * nDotL;

	// // Optional shadow computation (if you implement cascaded shadow maps, for example)
	// float shadow = calcDirLightShadows(fragPos, normal, index); 
	// radiance *= (1.0 - shadow);

	return radiance;
}


vec3 CalcPointLight(uint index, vec3 normal, vec3 fragPos,
					vec3 viewDir, vec3 albedo, float rough,
					float metal, vec3 F0,  float viewDistance){

	if (pointLights[index].position.w == 0.0) {return vec3(0.0);}
	//Point light basics
	vec3 position = pointLights[index].position.xyz;
	vec3 color    = 10.0 * pointLights[index].color.rgb * pointLights[index].color.w;
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

vec3 CalcSpotLight(uint index, vec3 normal, vec3 fragPos,
                   vec3 viewDir, vec3 albedo, float rough,
                   float metal, vec3 F0, float viewDistance) {

    if (spotLights[index].position.w == 0.0) return vec3(0.0);

    // Spotlight properties
    vec3 position   = spotLights[index].position.xyz;
    vec3 direction  = normalize(spotLights[index].direction.xyz);
    float innerCut  = cos(spotLights[index].innerCutoff);
    float outerCut  = cos(spotLights[index].outerCutoff);
    float radius    = spotLights[index].direction.w; // w component of direction is range.

    // Light vector
    vec3 lightDir = normalize(position - fragPos);
    vec3 halfway  = normalize(lightDir + viewDir);
    float nDotV = max(dot(normal, viewDir), 0.0);
    float nDotL = max(dot(normal, lightDir), 0.0);
    float distance = length(position - fragPos);

    // Angle between light direction and fragment-to-light vector
    float theta = dot(lightDir, -direction); // -direction because light points from source to target
    float epsilon = innerCut - outerCut;
    float intensity = clamp((theta - outerCut) / epsilon, 0.0, 1.0);

    // Attenuation
    float attenuation = pow(clamp(1.0 - pow((distance / radius), 4.0), 0.0, 1.0), 2.0)
                        / (1.0 + (distance * distance));
    attenuation *= intensity;

    // Radiance
    vec3 color = 10.0 * spotLights[index].color.rgb * spotLights[index].color.w;
    vec3 radianceIn = color * attenuation;

    // Cook-Torrance BRDF
    float NDF = DistributionGGX(normal, halfway, rough);
    float G = GeometrySmith(normal, viewDir, lightDir, rough);
    vec3 F = fresnelSchlick(max(dot(halfway, viewDir), 0.0), F0);

    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - metal;

    vec3 numerator = NDF * G * F;
    float denominator = 4.0 * nDotV * nDotL;
    vec3 specular = numerator / max(denominator, 0.0000001);

    vec3 radiance = (kD * (albedo / PI) + specular) * radianceIn * nDotL;

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

vec4 GetDiffuse() {
	vec4 result = mix(material.albedo, texture(material.DIFFUSE_TEX, texCoords), float(material.useDiffuseMap));
	//result.xyz = GammaCorrect(result.xyz);
	return result;
}

float GetRoughness() {
	float result = mix(material.roughness, texture(material.ROUGHNESS_TEX, texCoords).g, float(material.useRoughnessMap));
	return result;
}

float GetMetallic() {
	float result = mix(material.roughness, texture(material.ROUGHNESS_TEX, texCoords).b, float(material.useRoughnessMap));
	return result;
}

vec3 GetEmission() {
	vec3 result = mix(material.emission, texture(material.EMISSION_TEX, texCoords).xyz, float(material.useEmissionMap));
	return result;
}


vec3 GetNormal(vec3 N) {
	mat3 toWorld = mat3(Tangent, BiTangent, N); 
	vec3 normalMap = normalize(texture(material.NORMAL_TEX, texCoords).rgb * 2.0 - 1.0);
	normalMap = toWorld * normalMap;
	return mix(N, normalMap, float(material.useNormalMap));

}