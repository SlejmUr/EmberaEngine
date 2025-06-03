#version 440 core
layout(points) in;
layout(line_strip, max_vertices = 24) out;

struct Cluster {
    vec4 minPoint;
    vec4 maxPoint;
    uint count;
    int lightIndices[100];
};

layout(std430, binding = 2) buffer ClusterBuffer {
    Cluster clusters[];
};

uniform mat4 W_VIEW_MATRIX;
uniform mat4 W_PROJECTION_MATRIX;

flat out vec3 color;

vec4 Transform(vec3 pos)
{
    return W_PROJECTION_MATRIX * vec4(pos, 1.0);
}

void DrawBox(vec3 minP, vec3 maxP)
{
    vec3 v0 = vec3(minP.x, minP.y, minP.z);
    vec3 v1 = vec3(maxP.x, minP.y, minP.z);
    vec3 v2 = vec3(maxP.x, maxP.y, minP.z);
    vec3 v3 = vec3(minP.x, maxP.y, minP.z);

    vec3 v4 = vec3(minP.x, minP.y, maxP.z);
    vec3 v5 = vec3(maxP.x, minP.y, maxP.z);
    vec3 v6 = vec3(maxP.x, maxP.y, maxP.z);
    vec3 v7 = vec3(minP.x, maxP.y, maxP.z);

    // Draw front face
    gl_Position = Transform(v0); EmitVertex();
    gl_Position = Transform(v1); EmitVertex();
    gl_Position = Transform(v2); EmitVertex();
    gl_Position = Transform(v3); EmitVertex();
    gl_Position = Transform(v0); EmitVertex();
    EndPrimitive();

    // Draw back face
    gl_Position = Transform(v4); EmitVertex();
    gl_Position = Transform(v5); EmitVertex();
    gl_Position = Transform(v6); EmitVertex();
    gl_Position = Transform(v7); EmitVertex();
    gl_Position = Transform(v4); EmitVertex();
    EndPrimitive();

    // Connect edges
    gl_Position = Transform(v0); EmitVertex();
    gl_Position = Transform(v4); EmitVertex(); EndPrimitive();

    gl_Position = Transform(v1); EmitVertex();
    gl_Position = Transform(v5); EmitVertex(); EndPrimitive();

    gl_Position = Transform(v2); EmitVertex();
    gl_Position = Transform(v6); EmitVertex(); EndPrimitive();

    gl_Position = Transform(v3); EmitVertex();
    gl_Position = Transform(v7); EmitVertex(); EndPrimitive();
}

void main()
{
    uint clusterID = gl_PrimitiveIDIn;
    Cluster c = clusters[clusterID];

    color = vec3(float(clusterID % 256) / 255.0, float((clusterID * 47) % 256) / 255.0, 1.0);
    DrawBox(c.minPoint.xyz, c.maxPoint.xyz);
}
