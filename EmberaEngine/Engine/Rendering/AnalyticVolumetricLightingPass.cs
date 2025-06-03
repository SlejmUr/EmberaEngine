using EmberaEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{
    internal class AnalyticVolumetricLightingPass : IRenderPass
    {
        Framebuffer AVLFB;

        Texture AVLTexture;

        Shader AVLShader;

        bool isActive = true;

        public bool GetState() => isActive;
        public void SetState(bool value) => isActive = value;

        public void Initialize(int width, int height)
        {
            AVLTexture = new Texture(TextureTarget2d.Texture2D);
            AVLTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            AVLFB = new Framebuffer("AnalyticVolumetricLighting Framebuffer");
            AVLFB.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, AVLTexture);

            AVLShader = new Shader("Engine/Content/Shaders/3D/basic/fullscreen.vert", "Engine/Content/Shaders/3D/Volumetrics/volumetricLighting.frag");

            AVLShader.Set("gDepth", 0);
            AVLShader.Set("gPosition", 1);
        }

        public void Apply(FrameData frameData)
        {
            AVLFB.Bind();

            AVLShader.Use();

            AVLShader.SetVector3("C_VIEWPOS", frameData.Camera.position);
            AVLShader.SetFloat("zNear", frameData.Camera.nearClip);
            AVLShader.SetFloat("zFar", frameData.Camera.farClip);

            AVLShader.Apply();

            GraphicsState.SetTextureActiveBinding(TextureUnit.Texture0);
            frameData.GBuffer.GetFramebufferTexture(2).Bind();
            GraphicsState.SetTextureActiveBinding(TextureUnit.Texture1);
            frameData.GBuffer.GetFramebufferTexture(1).Bind();

            GraphicsState.SetCulling(false);
            

            Graphics.DrawFullScreenTri();

            GraphicsState.SetCulling(true);

        }

        public Framebuffer GetOutputFramebuffer()
        {
            return AVLFB;
        }

        public void Resize(int width, int height)
        {
            throw new NotImplementedException();
        }
    }
}
