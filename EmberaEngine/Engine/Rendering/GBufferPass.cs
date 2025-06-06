﻿using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenTK.Graphics.OpenGL.GL;

namespace EmberaEngine.Engine.Rendering
{
    internal class GBufferPass : IRenderPass
    {
        Framebuffer GeometryFB;

        Texture NormalTexture;
        Texture DepthTexture;
        Texture PositionTexture;

        Shader gBufferShader;

        bool isActive = true;

        public bool GetState() => isActive;
        public void SetState(bool value) => isActive = value;

        public void Initialize(int width, int height)
        {

            NormalTexture = new Texture(TextureTarget2d.Texture2D);
            NormalTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            NormalTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);

            PositionTexture = new Texture(TextureTarget2d.Texture2D);
            PositionTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            PositionTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            PositionTexture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

            DepthTexture = new Texture(TextureTarget2d.Texture2D);
            DepthTexture.TexImage2D(width, height, PixelInternalFormat.Depth24Stencil8, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            DepthTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);

            GeometryFB = new Framebuffer("Geometry Buffer");
            GeometryFB.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, NormalTexture);
            GeometryFB.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment1, PositionTexture);
            GeometryFB.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.DepthStencilAttachment, DepthTexture);

            GeometryFB.SetDrawBuffers(
            [
                OpenTK.Graphics.OpenGL.DrawBuffersEnum.ColorAttachment0,
                OpenTK.Graphics.OpenGL.DrawBuffersEnum.ColorAttachment1
            ]);

            gBufferShader = new Shader("Engine/Content/Shaders/3D/basic/gbuffer");
        }
        public void Apply(FrameData frameData)
        {
            frameData.GBuffer = GeometryFB;
            if (!isActive) return;
            GeometryFB.Bind();
            Renderer3D.ApplyPerFrameSettings(frameData.Camera);

            List<Mesh> meshes = frameData.Meshes;

            gBufferShader.Use();

            for (int i = 0; i < meshes.Count; i++)
            {
                Mesh mesh = meshes[i];

                Matrix4 model = mesh.worldMatrix;


                // Change this to not compute model matrices every render pass, but rather a centralized compute for it, and preferably a UBO for projection and view
                gBufferShader.SetMatrix4("W_VIEW_MATRIX", frameData.Camera.GetViewMatrix());
                gBufferShader.SetMatrix4("W_PROJECTION_MATRIX", frameData.Camera.GetProjectionMatrix());
                gBufferShader.SetMatrix4("W_MODEL_MATRIX", model);
                gBufferShader.Apply();
                mesh.Draw();
            }
        }


        public Framebuffer GetOutputFramebuffer()
        {
            return GeometryFB;
        }

        public void Resize(int width, int height)
        {
            NormalTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            NormalTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);

            PositionTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            PositionTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            PositionTexture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

            DepthTexture.TexImage2D(width, height, PixelInternalFormat.Depth24Stencil8, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            DepthTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        }
    }
}
