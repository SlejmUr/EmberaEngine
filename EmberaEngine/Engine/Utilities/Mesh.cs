using System;
using System.Collections.Generic;

using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using OpenTK.Mathematics;

namespace EmberaEngine.Engine.Utilities
{
    public class Mesh : IDisposable
    {

        public VertexBuffer VBO;
        public IndexBuffer IBO;
        public VertexArray VAO;

        Vertex[] Vertices;

        public uint MaterialIndex;

        public int MeshID;
        public int VertexCount;
        public string path;
        public string name;
        public string fileID;
        public bool Renderable = true;

        internal Vector3 position;
        internal Vector3 rotation;
        internal Vector3 scale;

        bool IsStatic = true;

        bool isdisposed = false;

        public Mesh()
        {
        }

        ~Mesh()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (isdisposed) { return; }
            VBO?.Dispose();
            IBO?.Dispose();
            VAO?.Dispose();
            isdisposed = true;
        }

        public void Draw()
        {
            if (!Renderable || VAO == null) {
                Console.WriteLine("Non renderable"); 
                return; 
            }

            if (IBO == null)
            {
                VAO.Render();
            } else
            {
                VAO.Render(IBO);
            }
        }

        public void SetPath(string path)
        {
            this.path = path;
        }

        public string GetPath()
        {
            return path;
        }

        public void SetStatic(bool value)
        {
            IsStatic = value;
        }

        public void SetVertices(Vertex[] vertices)
        {
            VBO = new VertexBuffer(Vertex.VertexInfo, vertices.Length, IsStatic);
            VBO.SetData(vertices, vertices.Length);
            VAO = new VertexArray(VBO);
            this.VertexCount = vertices.Length;
            this.Vertices = vertices;
        }

        public void SetVertexArrayObject(VertexArray vao)
        {
            VAO = vao;
        }

        public void SetIndices(int[] indices)
        {
            IBO = new IndexBuffer(indices.Length, IsStatic);
            IBO.SetData(indices, indices.Length);
        }

        public Vertex[] GetVertices()
        {
            return Vertices;
        }
    }
}
