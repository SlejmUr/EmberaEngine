using EmberaEngine.Engine.Core;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{
    class AntiAliasPass : IRenderPass
    {
        Framebuffer fxaaBuffer;

        Texture outputTexture;

        Shader fxaaShader;

        Vector2 screenSize;

        bool isActive;

        public void Initialize(int width, int height)
        {
            screenSize = new Vector2(width, height);

            outputTexture = new Texture(TextureTarget2d.Texture2D);
            outputTexture.TexImage2D(width, height, PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            outputTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            outputTexture.GenerateMipmap();

            fxaaBuffer = new Framebuffer("FXAA Buffer");
            fxaaBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, outputTexture);

            fxaaShader = new Shader("Engine/Content/Shaders/3D/basic/fullscreen.vert", "Engine/Content/Shaders/3D/AA/fxaa.frag");
        }

        public void Apply(FrameData frameData)
        {
            Vector2 texelStep = new Vector2(1.0f /  screenSize.X, 1.0f / screenSize.Y);

            fxaaBuffer.Bind();
            GraphicsState.Clear(true);
            GraphicsState.SetCulling(false);
            GraphicsState.SetDepthTest(false);

            fxaaShader.Use();
            fxaaShader.SetInt("INPUT_TEXTURE", 0);
            fxaaShader.SetVector2("TEXEL_STEP", texelStep);
            fxaaShader.SetInt("FXAA_ON", isActive ?  1 : 0);

            GraphicsState.SetTextureActiveBinding(TextureUnit.Texture0);
            Renderer3D.GetComposite().GetFramebufferTexture(0).Bind();

            Graphics.DrawFullScreenTri();
            GraphicsState.SetDepthTest(true);
            GraphicsState.SetCulling(true);



        }

        public Framebuffer GetOutputFramebuffer()
        {
            return fxaaBuffer;
        }

        public bool GetState()
        {
            return isActive;
        }

        public void Resize(int width, int height)
        {
            screenSize = new Vector2(width, height);

            outputTexture.TexImage2D(width, height, PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            outputTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            outputTexture.GenerateMipmap();
        }

        public void SetState(bool value)
        {
            this.isActive = value;
        }
    }
}
