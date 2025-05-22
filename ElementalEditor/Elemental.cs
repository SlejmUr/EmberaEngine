﻿using ElementalEditor.Editor;
using EmberaEngine.Engine.Core;

namespace ElementalEditor
{
    public class Elemental
    {
        public Elemental()
        {

        }

        public void Run()
        {
            Application app = new Application();

            ApplicationSpecification specification = new ApplicationSpecification()
            {
                Name = "Hello World",
                Height = 1080,
                Width = 1920,
                forceVsync = true,
                useImGui = true,
                useImGuiDock = true,
                useCustomTitlebar = false
            };

            EditorLayer layer = new EditorLayer();
            layer.app = app;

            app.Create(specification);


            app.AddLayer(layer);

            app.Run();
        }
    }
}