using EmberaEngine.Engine.Core;
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

        public void Initialize(int width, int height)
        {
            outputTexture = new Texture(TextureTarget2d.Texture2D);
            outputTexture.TexImage2D(width, height, PixelInternalFormat.Rgb32f, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
            outputTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);

            fxaaBuffer = new Framebuffer("FXAA Buffer");
            fxaaBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, outputTexture);
        }

        public void Apply(FrameData frameData)
        {

        }

        public Framebuffer GetOutputFramebuffer()
        {
            throw new NotImplementedException();
        }

        public bool GetState()
        {
            throw new NotImplementedException();
        }

        public void Resize(int width, int height)
        {
            throw new NotImplementedException();
        }

        public void SetState(bool value)
        {
            throw new NotImplementedException();
        }
    }
}
