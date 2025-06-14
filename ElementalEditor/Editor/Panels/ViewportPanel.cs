using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Utilities;
using ImGuiNET;
using MaterialIconFont;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using OpenTK.Windowing.Desktop;
using ElementalEditor.Editor.Utils;
using Newtonsoft.Json.Serialization;

namespace ElementalEditor.Editor.Panels
{

    class ViewportPanel : Panel
    {
        struct SupportedResolution
        {
            public int height, width;
            public int refreshRate;
        }


        int toolbarHeight = 48;



        List<SupportedResolution> SupportedResolutions = new List<SupportedResolution>();
        SupportedResolution selectedResolution;
        bool freeAspectRatio = false;
        (int, int, int, int) resizeCoords;

        Texture viewportBufferTexture;
        Framebuffer viewportBuffer;
        Framebuffer compositeBuffer;

        Shader viewportBlitShader;


        int prevViewportHeight, prevViewportWidth;
        int viewportHeight, viewportWidth;
        Vector2 viewportPos;

        bool isMouseOverWindow;

        public override void OnAttach()
        {
            compositeBuffer = Renderer3D.GetOutputFrameBuffer();

            viewportBufferTexture = new Texture(TextureTarget2d.Texture2D);
            viewportBufferTexture.TexImage2D(viewportWidth, viewportHeight, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            viewportBufferTexture.GenerateMipmap();

            viewportBuffer = new Framebuffer("VIEWPORT FB");
            viewportBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, viewportBufferTexture);

            viewportBlitShader = new Shader("Engine/Content/Shaders/3D/basic/fullscreen.vert", "Editor/Assets/Shaders/viewportBlitShader.frag");

            // FIX THIS, THIS ONLY ACCOUNTS FOR THE FIRST MONITOR!!!
            MonitorInfo monitorInfoList = Monitors.GetMonitors()[0];

            for (int i = 0; i < monitorInfoList.SupportedVideoModes.Count; i++)
            {
                if (monitorInfoList.SupportedVideoModes[i].RefreshRate != 60) continue;
                SupportedResolutions.Add(new SupportedResolution()
                {
                    width = monitorInfoList.SupportedVideoModes[i].Width,
                    height = monitorInfoList.SupportedVideoModes[i].Height,
                    refreshRate = monitorInfoList.SupportedVideoModes[i].RefreshRate
                });
            }

            for (int i = 0; i < monitorInfoList.SupportedVideoModes.Count; i++)
            {
                if (SupportedResolutions[i].width == monitorInfoList.HorizontalResolution && SupportedResolutions[i].height == monitorInfoList.VerticalResolution)
                {
                    selectedResolution = SupportedResolutions[i];
                    //selectedResolution.width = 1920;
                    //selectedResolution.height = 1080;
                    Renderer.Resize(selectedResolution.width, selectedResolution.height);
                    Screen.Size.X = selectedResolution.width;
                    Screen.Size.Y = selectedResolution.height;
                    break;
                }
            }

            Guizmo3D.Initialize();
        }
        public override void OnGUI()
        {

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(0, 0));
            ImGui.Begin(MaterialDesign.Landscape + " Viewport", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);


            viewportPos = ImGui.GetCursorScreenPos();

            viewportHeight = (int)ImGui.GetContentRegionAvail().Y;
            viewportWidth = (int)ImGui.GetContentRegionMax().X;

            if (prevViewportHeight != viewportHeight || prevViewportWidth != viewportWidth)
            {
                prevViewportHeight = viewportHeight;
                prevViewportWidth = viewportWidth;

                Console.WriteLine(viewportWidth);

                viewportBufferTexture.TexImage2D(viewportWidth, viewportHeight, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                viewportBufferTexture.GenerateMipmap();
            
            }
            resizeCoords = CalculateScaleWithScreen(viewportWidth, viewportHeight, selectedResolution.width, selectedResolution.height);

            if (!freeAspectRatio)
            {
                ClearViewport();
                Framebuffer.BlitFrameBuffer(compositeBuffer, viewportBuffer, (0, 0, selectedResolution.width, selectedResolution.height), (resizeCoords.Item1, resizeCoords.Item2, resizeCoords.Item3, resizeCoords.Item4), OpenTK.Graphics.OpenGL.ClearBufferMask.ColorBufferBit, OpenTK.Graphics.OpenGL.BlitFramebufferFilter.Nearest);
                //RenderViewport();
            } else
            {
                Framebuffer.BlitFrameBuffer(compositeBuffer, viewportBuffer, (0, 0, selectedResolution.width, selectedResolution.height), (resizeCoords.Item1, resizeCoords.Item2, resizeCoords.Item3, resizeCoords.Item4), OpenTK.Graphics.OpenGL.ClearBufferMask.ColorBufferBit, OpenTK.Graphics.OpenGL.BlitFramebufferFilter.Nearest);

            }

            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(0, 0));
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new System.Numerics.Vector2(0, 0));
            ImGui.Image((IntPtr)viewportBuffer.GetFramebufferTexture(0).GetRendererID(), new System.Numerics.Vector2(ImGui.GetContentRegionMax().X,ImGui.GetContentRegionAvail().Y), new System.Numerics.Vector2(0, 0), new System.Numerics.Vector2(1, -1));


            ImGui.PopStyleVar(2);

            ImGui.SetItemAllowOverlap();

            DrawViewportTools();

            isMouseOverWindow = ImGui.IsWindowHovered();

            ImGui.End();

            ImGui.PopStyleVar();
        }




        public (int, int, int, int) CalculateScaleWithScreen(int viewportWidth,int viewportHeight,int resolutionWidth,int resolutionHeight)
        {
            int usableHeight = viewportHeight - toolbarHeight;

            // Calculate scale maintaining aspect ratio
            float scaleWidth = (float)viewportWidth / resolutionWidth;
            float scaleHeight = (float)usableHeight / resolutionHeight;
            float scaleFactor = Math.Min(scaleWidth, scaleHeight);

            // Scaled dimensions
            float scaledWidth = resolutionWidth * scaleFactor;
            float scaledHeight = resolutionHeight * scaleFactor;

            // Center horizontally in full width, vertically in usable height
            int left = (int)((viewportWidth - scaledWidth) / 2);
            int top = (int)((usableHeight - scaledHeight) / 2);  // now correctly centers below toolbar
            int right = left + (int)scaledWidth;
            int bottom = top + (int)scaledHeight;

            return (left, top, right, bottom);
        }

        static float MapValue(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            // First, normalize the value to the range [0, 1] within the source range
            float normalizedValue = ((value - fromMin) * (toMax - toMin)) / (fromMax - fromMin) + toMin;

            return normalizedValue;

        }

        public override void OnKeyDown(KeyboardEvent key)
        {
            if (isMouseOverWindow)
            {
                Input.OnKeyDown(key.Key);
            }
        }

        public override void OnKeyUp(KeyboardEvent key)
        {
            if (isMouseOverWindow)
            {
                Input.OnKeyUp(key.Key);
            }
        }

        public override void OnMouseMove(MouseMoveEvent moveEvent)
        {

            if (editor.EditorCurrentScene.IsPlaying)
            {
                if (freeAspectRatio)
                {
                    moveEvent.position.X = (int)MapValue(moveEvent.position.X - viewportPos.X, 0, editor.app.window.Size.X, 0, viewportWidth);
                    moveEvent.position.Y = (int)MapValue(moveEvent.position.Y - (viewportPos.Y + 46), 0, editor.app.window.Size.Y, 0, viewportHeight);
                } else
                {
                    moveEvent.position.X = (int)MapValue(moveEvent.position.X, viewportPos.X + resizeCoords.Item1, viewportPos.X + resizeCoords.Item3, 0, selectedResolution.width);
                    moveEvent.position.Y = (int)MapValue(moveEvent.position.Y, viewportPos.Y + resizeCoords.Item2, viewportPos.Y + resizeCoords.Item4, selectedResolution.height, 0);
                }

                Input.OnMouseMove(moveEvent);
            }
        }

        public override void OnMouseWheel(MouseWheelEvent mouseWheel)
        {
            if (isMouseOverWindow)
            {
                Input.OnMouseWheel(mouseWheel);
            }
        }

        public override void OnMouseButton(MouseButtonEvent buttonEvent)
        {
            if (editor.EditorCurrentScene.IsPlaying)
            {

            }
        }

        public override void OnRender()
        {

            if (freeAspectRatio && (prevViewportHeight != viewportHeight || prevViewportWidth != viewportWidth))
            {
                Screen.Size.X = viewportWidth;
                Screen.Size.Y = viewportHeight;
                Renderer.Resize(viewportWidth, viewportHeight);
                editor.EditorCurrentScene.OnResize(viewportWidth, viewportHeight);
            }
        }

        public void RenderViewport()
        {
            
            //Framebuffer.BlitFrameBuffer(compositeBuffer, viewportBuffer, (0, 0, selectedResolution.width, selectedResolution.height), (resizeCoords.Item1, resizeCoords.Item2, resizeCoords.Item3, resizeCoords.Item4), OpenTK.Graphics.OpenGL.ClearBufferMask.ColorBufferBit, OpenTK.Graphics.OpenGL.BlitFramebufferFilter.Nearest);

            //ClearViewport();

            viewportBuffer.Bind();
            GraphicsState.SetViewport(0, 0, viewportWidth, viewportHeight);
            GraphicsState.ClearColor(0, 0, 0, 1);
            GraphicsState.Clear(true);
            GraphicsState.SetCulling(false);
            viewportBlitShader.Use();

            // 🟢 SOURCE — the full texture
            OpenTK.Mathematics.Vector4 sourceDimensions = new OpenTK.Mathematics.Vector4(
                0f, 0f,
                1f, 1f // full texture UVs
            );

            // 🟢 DESTINATION — the portion of the screen to blit into
            OpenTK.Mathematics.Vector4 destinationDimensions = new OpenTK.Mathematics.Vector4(
                resizeCoords.Item1 / (float)viewportWidth,
                resizeCoords.Item2 / (float)viewportHeight,
                resizeCoords.Item3 / (float)viewportWidth,
                resizeCoords.Item4 / (float)viewportHeight
            );


            viewportBlitShader.SetVector4("sourceDimensions", sourceDimensions);
            viewportBlitShader.SetVector4("destinationDimensions", destinationDimensions);
            viewportBlitShader.SetInt("INPUT_TEXTURE", 0);

            GraphicsState.SetTextureActiveBinding(TextureUnit.Texture0);
            Renderer3D.GetOutputFrameBuffer().GetFramebufferTexture(0).Bind();

            Graphics.DrawFullScreenTri();

            //GraphicsState.SetDepthTest(true);
            GraphicsState.SetCulling(true);
            Framebuffer.Unbind();
        }

        public void DrawViewportTools()
        {
            float buttonSize = 40;

            // Style setup
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10, 10));
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(6, 4));
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(10, 8));
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 3f);

            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.16f, 0.15f, 0.13f, 1f));
            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.28f, 0.30f, 0.33f, 1f));
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.25f, 0.25f, 0.25f, 0.9f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.35f, 0.35f, 0.35f, 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.20f, 0.20f, 0.20f, 1f));

            // Begin tool child panel
            ImGui.SetCursorPos(new Vector2(1.5f, toolbarHeight));
            ImGui.BeginChild("##VIEWPORT_TOOLS", new Vector2(ImGui.GetContentRegionAvail().X - 1.5f, toolbarHeight), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysUseWindowPadding);

            // --- Resolution dropdown ---
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Resolution:");
            ImGui.SameLine();

            var resLabel = freeAspectRatio
                ? "Free Aspect"
                : $"{selectedResolution.width}x{selectedResolution.height} @ {selectedResolution.refreshRate}Hz";

            ImGui.PushItemWidth(200);

            if (ImGui.BeginCombo("##ResCombo", resLabel, ImGuiComboFlags.HeightLargest))
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0f, 0f, 0f, 1f));
                for (int i = 0; i < SupportedResolutions.Count; i++)
                {
                    var res = SupportedResolutions[i];
                    bool isSelected = selectedResolution.Equals(res) && !freeAspectRatio;
                    if (ImGui.Selectable($"{res.width}x{res.height}", isSelected))
                    {
                        selectedResolution = res;
                        freeAspectRatio = false;

                        Renderer.Resize(res.width, res.height);
                        Screen.Size.X = res.width;
                        Screen.Size.Y = res.height;
                        editor.EditorCurrentScene.OnResize(res.width, res.height);

                        DebugLogPanel.Log("RESIZED RENDERER", DebugLogPanel.DebugMessageSeverity.Information, "Viewport Change");
                    }
                }

                if (ImGui.Selectable("Free Aspect", freeAspectRatio))
                {
                    freeAspectRatio = true;
                    Renderer.Resize(viewportWidth, viewportHeight);
                    Screen.Size.X = viewportWidth;
                    Screen.Size.Y = viewportHeight;
                    editor.EditorCurrentScene.OnResize(viewportWidth, viewportHeight);
                }
                ImGui.PopStyleColor();
                ImGui.EndCombo();
            }

            ImGui.PopItemWidth();

            // --- Play/Pause button ---
            float centerX = ImGui.GetWindowContentRegionMax().X / 2;
            ImGui.SameLine();
            ImGui.SetCursorPosY(4);
            ImGui.SetCursorPosX(centerX - (buttonSize / 2));

            var icon = editor.EditorCurrentScene.IsPlaying ? MaterialDesign.Pause : MaterialDesign.Play_arrow;
            if (ImGui.Button(icon, new Vector2(buttonSize, buttonSize)))
            {
                // You can toggle play mode here
                editor.EditorCurrentScene.IsPlaying = !editor.EditorCurrentScene.IsPlaying;
            }

            ImGui.EndChild();

            // Pop all styles
            ImGui.PopStyleColor(5);
            ImGui.PopStyleVar(7);
        }


        public void ClearViewport()
        {
            viewportBuffer.Bind();
            GraphicsState.ClearColor(0,0,0,0);
            GraphicsState.Clear();
            Framebuffer.Unbind();
        }

    }
}
