using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{

    public class SceneObject
    {
        public Mesh mesh;
        public int drawIndex;
        public int rendererID;
        public Material[] materials;
    }

    public class ModelRenderer
    {
        internal static List<SceneObject> sceneObjects;
        

        public static void Initialize()
        {
            sceneObjects = new List<SceneObject>();
        
        }

        public static SceneObject AddMesh(Mesh mesh)
        {
            SceneObject sceneObject = new SceneObject();

            sceneObject.mesh = mesh;

            RenderGraph.CURRENT_MESH_COUNT += 1;
            sceneObject.rendererID = UtilRandom.Next(RenderGraph.MAX_MESH_COUNT);

            sceneObjects.Add(sceneObject);

            return sceneObject;
        }

        public static void RemoveMesh(SceneObject sceneObject)
        {
            sceneObjects.Remove(sceneObject);
            RenderGraph.CURRENT_MESH_COUNT -= 1;
        }


    }
}
