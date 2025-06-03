using ElementalEditor.Editor.Utils;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using ImGuiNET;
using MaterialIconFont;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Panels
{
    class MaterialPanel : Panel
    {

        public override void OnAttach()
        {

        }

        public override void OnGUI()
        {
            Dictionary<uint, Material> materials = MaterialManager.materials;
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 6);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(5, 5));
            if (ImGui.Begin("Material Editor"))
            {
                for (int i = 0; i < materials.Count; i++)
                {


                    ImGui.PushID(materials.Keys.ElementAt(i).ToString());
                    if ( ImGui.CollapsingHeader(MaterialDesign.Circle +  " Material " + i) )
                    {
                        Material material = materials.Values.ElementAt(i);

                        Vector4 albedo = material.GetVector4("material.albedo");
                        Vector3 emission = material.GetVector3("material.emission");
                        float emissionStr = material.GetFloat("material.emissionStr");
                        float roughness = material.GetFloat("material.roughness");
                        float metallic = material.GetFloat("material.metallic");

                        bool useDiffuseTexture = material.GetInt("material.useDiffuseMap") != 0;
                        bool useNormalTexture = material.GetInt("material.useNormalMap") != 0;
                        bool useRoughnessTexture = material.GetInt("material.useRoughnessMap") != 0;
                        bool useEmissionTexture = material.GetInt("material.useEmissionMap") != 0;

                        ImGui.TreePush();

                        UI.BeginPropertyGrid("##" + i);

                        if (useDiffuseTexture)
                        {
                            UI.BeginProperty("Diffuse Texture");
                            UI.PropertyTexture(material.GetTexture("material.DIFFUSE_TEX").GetRendererID());
                            UI.EndProperty();
                        }

                        UI.BeginProperty("Albedo");
                        UI.PropertyVector4(ref albedo);
                        UI.EndProperty();

                        UI.BeginProperty("Emission");
                        UI.PropertyVector3(ref emission);
                        UI.EndProperty();

                        UI.BeginProperty("Emission Strength");
                        UI.PropertyFloat(ref emissionStr, 0, 10, 0.1f);
                        UI.EndProperty();

                        UI.BeginProperty("Roughness");
                        UI.PropertyFloat(ref roughness, 0, 1, 0.01f);
                        UI.EndProperty();

                        UI.BeginProperty("Metallic");
                        UI.PropertyFloat(ref metallic, 0, 1, 0.01f);
                        UI.EndProperty();

                        UI.BeginProperty("Use Diffuse Texture");
                        UI.PropertyBool(ref useDiffuseTexture);
                        UI.EndProperty();

                        UI.BeginProperty("Use Normal Texture");
                        UI.PropertyBool(ref useNormalTexture);
                        UI.EndProperty();

                        UI.BeginProperty("Use Roughness Texture");
                        UI.PropertyBool(ref useRoughnessTexture);
                        UI.EndProperty();

                        UI.BeginProperty("Use Emission Texture");
                        UI.PropertyBool(ref useEmissionTexture);
                        UI.EndProperty();

                        UI.EndPropertyGrid();

                        material.Set("material.albedo", albedo);
                        material.Set("material.emission", emission);
                        material.Set("material.emissionStr", emissionStr);
                        material.Set("material.roughness", roughness);
                        material.Set("material.metallic", metallic);
                        material.Set("material.useDiffuseMap", useDiffuseTexture ?  1 : 0);
                        material.Set("material.useNormalMap", useNormalTexture ? 1 : 0);
                        material.Set("material.useRoughnessMap", useRoughnessTexture ? 1 : 0);
                        material.Set("material.useEmissionMap", useEmissionTexture ? 1 : 0);

                        ImGui.TreePop();

                    }
                    ImGui.PopID();

                }




                ImGui.End();
                ImGui.PopStyleVar(2);
            }
        }

    }
}
