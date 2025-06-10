using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Core
{
    public static class ShaderRegistry
    {
        static Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();


        static ShaderRegistry()
        {
            shaders["CLUSTERED_PBR"] = new Shader("Engine/Content/Shaders/3D/PBR/clustered_pbr");
        }

        public static void RegisterShader(string name, Shader shader)
        {

            shaders.Add(name, shader);
        }

        public static Shader GetShader(string name)
        {
            if (shaders.TryGetValue(name, out Shader shader))
                return shader;
            else
                return null;
        }



    }
}
