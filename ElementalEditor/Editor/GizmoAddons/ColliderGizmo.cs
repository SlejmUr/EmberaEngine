using ElementalEditor.Editor.Panels;
using ElementalEditor.Editor.Utils;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;

namespace ElementalEditor.Editor.GizmoAddons
{
    internal class ColliderGizmo : GizmoObject
    {

        public override Type ComponentType => typeof(ColliderComponent3D);

        private Texture lightTexture;

        public override void Initialize()
        {
            
        }

        public override void OnRender(Component component)
        {
            ColliderComponent3D lComponent = (ColliderComponent3D)component;
            if (lComponent.ColliderShape == ColliderShapeType.Box)
            {
                Guizmo3D.RenderCube(lComponent.gameObject.transform.position, lComponent.Size, lComponent.gameObject.transform.rotation);
            }
        }
    }
}
