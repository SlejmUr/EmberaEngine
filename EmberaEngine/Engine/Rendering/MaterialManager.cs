using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{

    public class MaterialManager
    {
        public static Dictionary<uint, Material> materials;

        internal static Material nullMaterial;

        public static void Initialize()
        {
            materials = new Dictionary<uint, Material>();

            //nullMaterial = new Material(new Shader("Engine/Content/Shaders/3D/basic/base"));
        }

        public static Material GetMaterial(uint materialId)
        {
            if (!materials.ContainsKey(materialId)) { return nullMaterial; }
            return materials[materialId];
        }

        public static uint AddMaterial(Material material)
        {       
            uint materialId = (uint)materials.Keys.Count;

            materials.Add(materialId, material);

            return materialId;
        }
    }
}
