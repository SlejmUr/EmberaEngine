using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Panels
{
    class PerformancePanel : Panel
    {

        float dt;

        public override void OnGUI()
        {

            if (ImGui.Begin("Performance"))
            {
                if (ImGui.BeginTable("##debuglog", 2, ImGuiTableFlags.BordersV))
                {
                    ImGui.TableSetupColumn("Category", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Performance", ImGuiTableColumnFlags.WidthFixed);

                    ImGui.TableHeadersRow();
                    ImGui.TableNextColumn();

                    ImGui.Text("Frame Rate");
                    ImGui.TableNextColumn();

                    ImGui.Text((1/dt).ToString());
                    ImGui.TableNextColumn();


                    ImGui.EndTable();

                    ImGui.End();
                }

            }
        }

        public override void OnUpdate(float dt)
        {
            this.dt = dt;
        }
    }
}
