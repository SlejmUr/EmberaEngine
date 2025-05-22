using EmberaEngine.Engine.Core;
using System;
using System.Collections.Generic;

using OpenTK.Mathematics;
using EmberaEngine.Engine.Utilities;

namespace EmberaEngine.Engine.Rendering
{
    public static class Graphics
    {

        public static Matrix4 CreateOrthographicCenter(float left, float right, float bottom, float top, float depthNear, float depthFar)
        {
            return Matrix4.CreateOrthographicOffCenter(
                    left,
                    right,
                    bottom,
                    top,
                    depthNear,
                    depthFar
            );
        }

        public static Matrix4 CreateOrthographic2D(float width, float height, float depthNear, float depthFar)
        {
            return Matrix4.CreateOrthographicOffCenter(
                    0,
                    width,
                    0,
                    height,
                    depthNear,
                    depthFar
            );
        }


        public static Mesh GetCube()
        {
            VertexArray CubeVAO;
            Vertex[] vertices1 = Primitives.GetCubeVertex();
            VertexBuffer CubeVBO = new VertexBuffer(Vertex.VertexInfo, vertices1.Length, true);
            CubeVBO.SetData(vertices1, vertices1.Length);
            CubeVAO = new VertexArray(CubeVBO);

            Mesh Cube = new();
            Cube.SetVertexArrayObject(CubeVAO);

            return Cube;
        }

    }
}
