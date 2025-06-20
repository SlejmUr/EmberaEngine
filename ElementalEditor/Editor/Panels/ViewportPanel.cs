﻿using ElementalEditor.Editor.Utils;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Utilities;
using ImGuiNET;
using MaterialIconFont;
using OpenTK.Windowing.Desktop;
using System.Numerics;

namespace ElementalEditor.Editor.Panels
{
    class ViewportPanel : Panel
    {
        struct SupportedResolution
        {
            public int Width, Height, RefreshRate;

            public override bool Equals(object obj) => obj is SupportedResolution r &&
                r.Width == Width && r.Height == Height && r.RefreshRate == RefreshRate;
        }

        const int ToolbarHeight = 60;
        List<SupportedResolution> supportedResolutions = new();
        SupportedResolution selectedResolution;

        Texture viewportTexture;
        Framebuffer viewportBuffer;
        Framebuffer compositeBuffer;
        Shader blitShader;

        Vector2 viewportPos;
        (int Left, int Top, int Right, int Bottom) scaledViewport;

        int prevViewportHeight, prevViewportWidth;
        int viewportHeight, viewportWidth;

        bool isMouseOverWindow;
        bool isFirstFrame;
        bool freeAspectRatio = false;


        List<ViewportControl> controls;


        public override void OnAttach()
        {
            // Setting up UI

            controls = new List<ViewportControl>
            {
                new ViewportControl(ViewportAlignment.Left, () =>
                {
                    if (ImGui.BeginCombo("##ResCombo", $"{selectedResolution.Width}x{selectedResolution.Height}", ImGuiComboFlags.HeightLargest))
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0f, 0f, 0f, 1f));
                        for (int i = 0; i < supportedResolutions.Count; i++)
                        {
                            var res = supportedResolutions[i];
                            bool isSelected = selectedResolution.Equals(res) && !freeAspectRatio;
                            if (ImGui.Selectable($"{res.Width}x{res.Height}", isSelected))
                            {
                                selectedResolution = res;
                                freeAspectRatio = false;

                                Renderer.Resize(res.Width, res.Height);
                                Screen.Size.X = res.Width;
                                Screen.Size.Y = res.Height;
                                editor.EditorCurrentScene.OnResize(res.Width, res.Height);

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
                }, new Vector2(200f)),
                new ViewportControl(ViewportAlignment.Center, () =>
                {
                    if (ImGui.Button(MaterialDesign.Play_arrow, new Vector2(40f)))
                        Console.WriteLine("Play clicked");
                }),

                new ViewportControl(
                    ViewportAlignment.Right,
                    () =>
                    {
                        ImGui.Button("Save Scene");
                    },
                    new Vector2(100, 40)
                ),

                new ViewportControl(ViewportAlignment.Right, () =>
                {
                    if (ImGui.Button("⋮", new Vector2(40f)))
                        ImGui.OpenPopup("OptionsPopup");

                    if (ImGui.BeginPopup("OptionsPopup"))
                    {
                        if (ImGui.MenuItem("Option A")) Console.WriteLine("Option A clicked");
                        if (ImGui.MenuItem("Option B")) Console.WriteLine("Option B clicked");
                        ImGui.EndPopup();
                    }
                })
            };






            compositeBuffer = Renderer3D.GetOutputFrameBuffer();

            InitViewportBuffer();
            blitShader = new Shader("Engine/Content/Shaders/3D/basic/fullscreen.vert", "Editor/Assets/Shaders/viewportBlitShader.frag");

            InitSupportedResolutions();
            SetInitialResolution();

            Guizmo3D.Initialize();
        }

        void InitViewportBuffer()
        {
            viewportTexture = new Texture(TextureTarget2d.Texture2D);
            viewportBuffer = new Framebuffer("VIEWPORT FB");
            viewportBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, viewportTexture);
        }

        void InitSupportedResolutions()
        {
            var videoModes = Monitors.GetMonitors()[0].SupportedVideoModes;
            foreach (var mode in videoModes)
            {
                if (mode.RefreshRate != 60) continue;
                supportedResolutions.Add(new SupportedResolution
                {
                    Width = mode.Width,
                    Height = mode.Height,
                    RefreshRate = mode.RefreshRate
                });
            }
        }

        void SetInitialResolution()
        {
            var monitor = Monitors.GetMonitors()[0];
            foreach (var res in supportedResolutions)
            {
                if (res.Width == monitor.HorizontalResolution && res.Height == monitor.VerticalResolution)
                {
                    selectedResolution = res;
                    selectedResolution.Width = 1920;
                    selectedResolution.Height = 1080;
                    ApplyResolution(selectedResolution.Width, selectedResolution.Height);
                    break;
                }
            }
        }

        public override void OnGUI()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            ImGui.Begin(MaterialDesign.Landscape + " Viewport", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            isMouseOverWindow = ImGui.IsWindowHovered();
            editor.EditorCamera.LockCamera = !isMouseOverWindow;

            // 📌 DRAW TOOLBAR FIRST
            DrawToolbar(); // This will now occupy vertical space in layout

            viewportPos = ImGui.GetCursorScreenPos();
            viewportHeight = (int)ImGui.GetContentRegionAvail().Y;
            viewportWidth = (int)ImGui.GetContentRegionMax().X;

            HandleViewportResize();

            scaledViewport = CalculateScaledViewport(viewportWidth, viewportHeight, selectedResolution.Width, selectedResolution.Height);

            RenderToViewport();

            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, Vector2.Zero);

            ImGui.Image((IntPtr)viewportBuffer.GetFramebufferTexture(0).GetRendererID(),
                        new Vector2(viewportWidth, viewportHeight),
                        new Vector2(0, 0), new Vector2(1, -1));

            ImGui.PopStyleVar(2);

            ImGui.End();
            ImGui.PopStyleVar();
        }


        void HandleViewportResize()
        {
            if (viewportHeight != prevViewportHeight || viewportWidth != prevViewportWidth)
            {
                prevViewportHeight = viewportHeight;
                prevViewportWidth = viewportWidth;

                viewportTexture.TexImage2D(viewportWidth, viewportHeight, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                viewportTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            }
        }

        void RenderToViewport()
        {
            var src = (0, 0, selectedResolution.Width, selectedResolution.Height);
            var dst = (scaledViewport.Left, scaledViewport.Top, scaledViewport.Right, scaledViewport.Bottom);

            if (!freeAspectRatio)
                ClearViewport();

            Framebuffer.BlitFrameBuffer(compositeBuffer, viewportBuffer, src, dst,
                OpenTK.Graphics.OpenGL.ClearBufferMask.ColorBufferBit,
                OpenTK.Graphics.OpenGL.BlitFramebufferFilter.Nearest);
        }

        void DrawToolbar()
        {
            // Extracted: Same as your `DrawViewportTools` but modularized — we can leave as is or optionally break down more.
            ViewportUtil.DrawViewportTools(ToolbarHeight, controls);
        }


        public override void OnKeyDown(KeyboardEvent key)
        {
            if (isMouseOverWindow)
                Input.OnKeyDown(key.Key);
        }

        public override void OnKeyUp(KeyboardEvent key)
        {
            Input.OnKeyUp(key.Key);
        }

        public override void OnMouseMove(MouseMoveEvent move)
        {
            if (!editor.EditorCurrentScene.IsPlaying) return;

            if (freeAspectRatio)
            {
                move.position.X = (int)MapValue(move.position.X - viewportPos.X, 0, editor.app.window.Size.X, 0, viewportWidth);
                move.position.Y = (int)MapValue(move.position.Y - (viewportPos.Y + 46), 0, editor.app.window.Size.Y, 0, viewportHeight);
            }
            else
            {
                move.position.X = (int)MapValue(move.position.X, viewportPos.X + scaledViewport.Left, viewportPos.X + scaledViewport.Right, 0, selectedResolution.Width);
                move.position.Y = (int)MapValue(move.position.Y, viewportPos.Y + scaledViewport.Top, viewportPos.Y + scaledViewport.Bottom, selectedResolution.Height, 0);
            }

            Input.OnMouseMove(move);
        }

        public override void OnMouseWheel(MouseWheelEvent wheel)
        {
            if (isMouseOverWindow)
                Input.OnMouseWheel(wheel);
        }

        public override void OnMouseButton(MouseButtonEvent button) { }

        public override void OnRender()
        {
            if (freeAspectRatio && (viewportWidth != prevViewportWidth || viewportHeight != prevViewportHeight))
            {
                ApplyResolution(viewportWidth, viewportHeight);
                editor.EditorCurrentScene.OnResize(viewportWidth, viewportHeight);
            }
        }

        void ApplyResolution(int width, int height)
        {
            Screen.Size.X = width;
            Screen.Size.Y = height;
            Renderer.Resize(width, height);
        }

        public void ClearViewport()
        {
            viewportBuffer.Bind();
            GraphicsState.ClearColor(0, 0, 0, 0);
            GraphicsState.Clear();
            Framebuffer.Unbind();
        }

        public (int, int, int, int) CalculateScaledViewport(int viewportW, int viewportH, int targetW, int targetH)
        {
            int usableHeight = viewportH;
            float scale = Math.Min((float)viewportW / targetW, (float)usableHeight / targetH);
            int sw = (int)(targetW * scale);
            int sh = (int)(targetH * scale);

            int left = (viewportW - sw) / 2;
            int top = (usableHeight - sh) / 2;
            return (left, top, left + sw, top + sh);
        }

        static float MapValue(float val, float inMin, float inMax, float outMin, float outMax)
        {
            return ((val - inMin) * (outMax - outMin)) / (inMax - inMin) + outMin;
        }
    }
}
