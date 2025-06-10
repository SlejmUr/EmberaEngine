using ElementalEditor.Editor.EditorAttributes;
using ElementalEditor.Editor.Utils;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Rendering;
using ImGuiNET;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.CustomEditors
{
    [CustomEditor(typeof(LightComponent))]
    class LightComponentEditor : CustomEditorScript
    {
        string[] lightTypes;

        public override void OnEnable()
        {
            lightTypes = typeof(LightType).GetEnumNames();
        }

        public override void OnGUI()
        {
            LightComponent lightComponent = (LightComponent)component;

            int lightType = (int)lightComponent.LightType;
            bool enabled = (bool)lightComponent.Enabled;
            Color4 color = lightComponent.Color;
            float intensity = lightComponent.Intensity;
            float radius = lightComponent.Radius;
            float innerCutoff = lightComponent.InnerCutoff;
            float outerCutoff = lightComponent.OuterCutoff;


            UI.BeginProperty("Light Type");
            if (UI.PropertyEnum(ref lightType, lightTypes, lightTypes.Length))
                lightComponent.LightType = (LightType)typeof(LightType).GetEnumValues().GetValue(lightType);
            UI.EndProperty();

            UI.BeginProperty("Enabled");
            if (UI.PropertyBool(ref enabled))
                lightComponent.Enabled = enabled;
            UI.EndProperty();

            UI.BeginProperty("Color");
            if (UI.PropertyColor4(ref color, true))
                lightComponent.Color = color;
            UI.EndProperty();

            UI.BeginProperty("Intensity");
            if (UI.PropertyFloat(ref intensity, 0))
                lightComponent.Intensity = intensity;
            UI.EndProperty();

            UI.BeginProperty("Radius");
            if (UI.PropertyFloat(ref radius))
                lightComponent.Radius = radius;
            UI.EndProperty();

            if (lightComponent.LightType == LightType.PointLight)
            {
            } else if (lightComponent.LightType == LightType.SpotLight)
            {

                UI.BeginProperty("Outer Cutoff");
                if (UI.PropertyFloat(ref outerCutoff, innerCutoff))
                    lightComponent.OuterCutoff = outerCutoff;
                UI.EndProperty();

                UI.BeginProperty("Inner Cutoff");
                if (UI.PropertyFloat(ref innerCutoff, 0, outerCutoff))
                    lightComponent.InnerCutoff = innerCutoff;
                UI.EndProperty();
            }
        }

    }
}
