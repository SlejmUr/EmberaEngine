using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{
    public abstract class BuiltInRenderer
    {
        public abstract void Initialize();
        public abstract void Render();
        public abstract void BeginRender();
        public abstract void EndRender();

    }
}
