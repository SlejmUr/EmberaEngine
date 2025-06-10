using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidShaderBuilder
{
    public class Sampler2D : ShaderStatement
    {
        string samplerName;


        public override void Initialize(string samplerName)
        {
            this.samplerName = samplerName;
        }
        public override string ToGLSL()
        {
            return "uniform sampler2D " + samplerName;
        }
    }
}
