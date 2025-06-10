using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{
    struct DrawElementsIndirectCommand 
    {
        public uint count;
        public uint instanceCount;
        public uint firstIndex;
        public int baseVertex;
        public uint baseInstance;
    }

    struct BasicMeshEntry
    {

    }




    // This is an internal render method that uses GPU-Driven rendering
    static class IndirectBatchedTechnique
    {
        static Dictionary<Shader, List<DrawElementsIndirectCommand>> drawCommands;
        static Dictionary<Shader, List<BufferObject<DrawElementsIndirectCommand>>> drawElements;


        static IndirectBatchedTechnique() {
            drawElements = new Dictionary<Shader, List<BufferObject<DrawElementsIndirectCommand>>>();
            drawCommands = new Dictionary<Shader, List<DrawElementsIndirectCommand>>();
        }

        static void CreateDrawCommandBuffer(Shader shader, List<Mesh> meshes)
        {
            drawCommands[shader] = new List<DrawElementsIndirectCommand>();

            for (int i = 0; i < meshes.Count; i++)
            {
                //drawCommands[shader].Add(new DrawElementsIndirectCommand()
                //{
                //    count = (uint)meshes[i].IBO.IndexCount,
                //    instanceCount = 1,
                //    firstIndex = 
                //});
            }
        }




    }
}
