using Assimp;
using ElementalEditor.Editor.GizmoAddons;
using ElementalEditor.Editor.Utils;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Panels
{
    public abstract class GizmoObject
    {
        public abstract Type ComponentType { get; }

        public abstract void Initialize();

        public abstract void OnRender(Component component);
    }



    class GuizmoPanel : Panel
    {
        public List<GizmoObject> GizmoObjects;

        public override void OnAttach()
        {
            GizmoObjects = new List<GizmoObject>();

            GizmoObjects.Add(new LightGizmo());
            GizmoObjects.Add(new ColliderGizmo());

            for (int i = 0; i < GizmoObjects.Count; i++)
            {
                GizmoObjects[i].Initialize();
            }
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
        }

        public override void OnLateRender()
        {
            Renderer3D.GetOutputFrameBuffer().Bind();

            Guizmo3D.Render();

            List<Component> components = editor.EditorCurrentScene.GetComponents();

            for (int j = 0; j < components.Count; j++)
            {
                for (int i = 0; i < GizmoObjects.Count; i++)
                {
                    if (GizmoObjects[i].ComponentType == components[j].GetType())
                    {
                        GizmoObjects[i].OnRender(components[j]);
                    }
                }
            }
        }

        public override void OnGUI()
        {
            if (ImGui.Begin("Guizmo Manager"))
            {


                ImGui.End();
            }
        }

    }
}
