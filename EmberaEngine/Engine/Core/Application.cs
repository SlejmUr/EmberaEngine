using EmberaEngine.Engine.Imgui;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Utilities;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace EmberaEngine.Engine.Core
{

    public struct ApplicationSpecification
    {
        public string Name;
        public int Width, Height;
        public bool forceVsync;
        public bool useImGui;
        public bool useImGuiDock;
        public bool useCustomTitlebar;
        public bool useFullscreen;
    }

    public class Application
    {
        public Window window;
        public LayerHandler LayerHandler;
        public ImguiLayer ImGuiLayer;
        public ApplicationSpecification appSpec;

        public void Create(ApplicationSpecification appSpec)
        {
            WindowSpecification windowSpec = new WindowSpecification()
            {
                Name = appSpec.Name,
                Width = appSpec.Width,
                Height = appSpec.Height,
                VSync = appSpec.forceVsync,
                customTitlebar = appSpec.useCustomTitlebar,
            };

            LayerHandler = new LayerHandler();

            if (appSpec.useImGui)
            {
                ImGuiLayer = new ImguiLayer();
                ImguiLayer.UseDockspace = appSpec.useImGuiDock;
                ImguiLayer.customTitlebar = appSpec.useCustomTitlebar;
                LayerHandler.AddLayer(ImGuiLayer);
            }

            window = new Window(windowSpec);

            window.Load += OnLoad;
            window.UpdateFrame += OnUpdateFrame;
            window.RenderFrame += OnRenderFrame;

            window.KeyDown += OnKeyDown;
            window.KeyUp += OnKeyUp;

            window.Resize += OnResize;
            window.TextInput += OnTextInput;

            window.MouseMove += OnMouseMove;
            window.MouseDown += OnMouseInput;
            window.MouseEnter += OnMouseEnter;
            window.MouseLeave += OnMouseLeave;
            window.MouseUp += OnMouseInput;
            window.MouseWheel += OnMouseWheel;

            Renderer.Initialize(appSpec.Width, appSpec.Height);

            this.appSpec = appSpec;
        }

        private void OnMouseMove(OpenTK.Windowing.Common.MouseMoveEventArgs obj)
        {
            LayerHandler.OnMouseMoveEvent(obj);
        }

        private void OnMouseWheel(OpenTK.Windowing.Common.MouseWheelEventArgs obj)
        {
            LayerHandler.OnMouseWheelEvent(obj);
        }

        private void OnMouseLeave()
        {

        }

        private void OnMouseEnter()
        {

        }

        private void OnMouseInput(OpenTK.Windowing.Common.MouseButtonEventArgs obj)
        {
            LayerHandler.OnMouseButtonEvent(obj);
        }

        public ImguiAPI? GetImGuiAPI()
        {
            if (appSpec.useImGui)
            {
                return ImGuiLayer.imguiAPI;
            }
            return null;
        }

        private void OnTextInput(OpenTK.Windowing.Common.TextInputEventArgs obj)
        {
            LayerHandler.TextInput(obj.Unicode);
        }

        private void OnResize(OpenTK.Windowing.Common.ResizeEventArgs obj)
        {
            LayerHandler.ResizeLayers(obj.Width, obj.Height);
        }

        private void OnLoad()
        {
            if (appSpec.useImGui)
            {
                ImGuiLayer.InitIMGUI(window, appSpec.Width, appSpec.Height);
            }

            LayerHandler.AttachLayers();
        }

        private void OnKeyDown(OpenTK.Windowing.Common.KeyboardKeyEventArgs obj)
        {
            bool caps = obj.Modifiers.ToString().Split(",").Contains("CapsLock");
            LayerHandler.KeyDownInput((Keys)(int)obj.Key, obj.ScanCode, obj.Modifiers.ToString(), caps);
        }

        private void OnKeyUp(OpenTK.Windowing.Common.KeyboardKeyEventArgs obj)
        {
            bool caps = obj.Modifiers.ToString().Split(",").Contains("CapsLock");
            LayerHandler.KeyUpInput((Keys)(int)obj.Key, obj.ScanCode, obj.Modifiers.ToString(), caps);
        }

        public void Run()
        {
            window.Run();
        }

        public void AddLayer(Layer layer)
        {
            LayerHandler.AddLayer(layer);
        }

        private void OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs obj)
        {
            LayerHandler.RenderLayers();
            Renderer.BeginFrame();
            Renderer.RenderFrame();
            Renderer.EndFrame();
            LayerHandler.LateRenderLayers();

        }

        private void OnUpdateFrame(OpenTK.Windowing.Common.FrameEventArgs obj)
        {
            if (appSpec.useImGui)
            {
                ImGuiLayer.Begin((float)obj.Time);
                for (int i = 0; i < LayerHandler.layers.Count; i++)
                {
                    LayerHandler.layers[i].OnGUIRender();
                }

                ImGuiLayer.End();
            }
            // Updating

            LayerHandler.UpdateLayers((float)obj.Time);

            // MOVE TO GAMELAYER.CS ON PROD/EXPORT FOR STANDALONE BUILDS
            UIManager.Update();
            Input.Update();
        }
    }
}
