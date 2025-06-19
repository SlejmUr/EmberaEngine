using ElementalEditor.Editor.Panels;
using ElementalEditor.Editor.Utils;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;

namespace ElementalEditor.Editor.GizmoAddons
{
    internal class LightGizmo : GizmoObject
    {

        public override Type ComponentType => typeof(LightComponent);

        private Texture pointLightTexture;
        private Texture spotLightTexture;
        private Texture directionalLightTexture;

        public override void Initialize()
        {
            pointLightTexture = Helper.loadImageAsTex("Editor/Assets/Textures/GizmoTextures/PointLightOverlay.png");
            spotLightTexture = Helper.loadImageAsTex("Editor/Assets/Textures/GizmoTextures/SpotLightOverlay.png");
            directionalLightTexture = Helper.loadImageAsTex("Editor/Assets/Textures/GizmoTextures/DirectionalLightOverlay.png");
        }

        public override void OnRender(Component component)
        {
            LightComponent lComponent = (LightComponent)component;
            if (!lComponent.Enabled) { return; }
            if (lComponent.LightType == EmberaEngine.Engine.Rendering.LightType.PointLight)
            {
                Guizmo3D.RenderTexture(pointLightTexture, component.gameObject.transform.Position, Vector3.One);
                Guizmo3D.RenderLightCircle(component.gameObject.transform.Position, Vector3.One * lComponent.Radius, Vector3.Zero);
            } else if (lComponent.LightType == EmberaEngine.Engine.Rendering.LightType.SpotLight)
            {
                Guizmo3D.RenderTexture(spotLightTexture, component.gameObject.transform.Position, Vector3.One);
            } else if (lComponent.LightType == EmberaEngine.Engine.Rendering.LightType.DirectionalLight)
            {
                Guizmo3D.RenderTexture(directionalLightTexture, component.gameObject.transform.Position, Vector3.One);
            }

        }
    }
}
