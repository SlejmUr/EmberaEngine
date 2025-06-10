using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidShaderBuilder
{
    public abstract class ShaderStatement
    {
        public abstract void Initialize(string name);
        public abstract string ToGLSL();


    }
}
