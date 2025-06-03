#version 440 core
layout(location = 0) in uint clusterID;

out uint clustId;

void main()
{
    // Pass the cluster ID to the geometry shader
    gl_Position = vec4(0.0); // not used
    gl_PointSize = 1.0;
    clustId = clusterID;
}
