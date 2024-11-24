using EmberaEngine.Engine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Components
{
    public class CanvasComponent : Component
    {
        public override string Type => nameof(CanvasComponent);

        CanvasObject co;

        public CanvasObject GetCanvasObject()
        {
            return co;
        }

        public override void OnStart()
        {
            co = CanvasRenderer.AddCanvasObject();
        }

        public override void OnUpdate(float dt)
        {

        }

        public override void OnDestroy()
        {
            CanvasRenderer.DestroyCanvasObject(co);
        }
    }
}
