using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{
    public static class RenderGraph
    {
        public enum SSBOBindIndex
        {
            LightBuffer = 0,
            LightGridBuffer = 3,
            ClusterBuffer = 4,
            GlobalLightIndexCount = 5,
            GlobalLightIndexList = 6,
            ScreenInfoBuffer = 7
            // VOLUMETRIC FOG TEXTURES
            // 8, 9, 10

        }

        // Above this limit, it is best to instance the meshes.
        public static int MAX_MESH_COUNT = 1000000;

        public static int CURRENT_MESH_COUNT = 0;

        public static int MAX_POINT_LIGHTS = 4096;
        public static int MAX_DIR_LIGHTS = 1;
        public static int MAX_SPOT_LIGHTS = 1024;

        public static int MAX_MATERIALS = 10000;


        public static bool isGraphicsContextInitialized = false;



    }
}
