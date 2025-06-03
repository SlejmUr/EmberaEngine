using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Panels
{
    class MenuBar : Panel
    {
        public override void OnAttach()
        {

        }

        public override void OnGUI()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Devoid"))
                {
                    if (ImGui.MenuItem("About Devoid"))
                    {

                    }


                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
        }

    }
}
