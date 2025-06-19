using EmberaEngine.Engine.Core;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{
    struct BloomMip
    {
        public Vector2i size;
        public Texture texture;
    }

    class BloomPass : IRenderPass
    {
        Framebuffer bloomFB;

        List<BloomMip> bloomMipList;

        int bloomMipCount = 8;

        Shader upsampleShader;
        Shader downsampleShader;

        Vector2 screenSize; // this is created here since i plan on having this pass reused for the 2D renderer. so i must have it renderer agnostic.

        bool karIsOnDownsample = true;
        float filterRadius = 0f;

        bool isActive = true;

        public bool GetState() => isActive;
        public void SetState(bool value) => isActive = value;

        public void Initialize(int width, int height)
        {
            screenSize = new Vector2(width, height);

            bloomMipList = new List<BloomMip>();

            Vector2 currentMipSize = new Vector2(width, height);

            for (int i = 0; i < bloomMipCount; i++)
            {
                BloomMip bloomMip = new BloomMip();

                currentMipSize *= 0.5f;
                if (currentMipSize.X <= 1 || currentMipSize.Y <= 1)
                {
                    continue;
                }

                bloomMip.size = (Vector2i)currentMipSize;

                Texture mipTex = new Texture(TextureTarget2d.Texture2D);
                mipTex.TexImage2D(bloomMip.size.X, bloomMip.size.Y, PixelInternalFormat.Rgb16f, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
                mipTex.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
                mipTex.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

                bloomMip.texture = mipTex;
                bloomMipList.Add(bloomMip);
            }

            bloomFB = new Framebuffer("Bloom FrameBuffer");
            bloomFB.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, bloomMipList[0].texture);
            bloomFB.SetDrawBuffers([OpenTK.Graphics.OpenGL.DrawBuffersEnum.ColorAttachment0]);

            upsampleShader = new Shader("Engine/Content/Shaders/3D/Bloom/bloom.vert", "Engine/Content/Shaders/3D/Bloom/upsample.frag");
            downsampleShader = new Shader("Engine/Content/Shaders/3D/Bloom/bloom.vert", "Engine/Content/Shaders/3D/Bloom/downsample.frag");

            upsampleShader.Use();
            upsampleShader.SetInt("INPUT_TEXTURE", 0);
            upsampleShader.Apply();

            downsampleShader.Use();
            downsampleShader.SetInt("INPUT_TEXTURE", 0);
            downsampleShader.Apply();
        }

        public void Apply(FrameData frameData)
        {
            if (!isActive) return;
            Texture inputTexture = frameData.EffectFrameBuffer.GetFramebufferTexture(1);

            bloomFB.Bind();
            GraphicsState.SetDepthTest(false);
            GraphicsState.SetCulling(false);
            GraphicsState.SetBlending(false);

            RenderDownsamples(inputTexture);
            RenderUpsamples();

            GraphicsState.SetDepthTest(true);
            GraphicsState.SetCulling(true);
            GraphicsState.SetBlending(true);

            GraphicsState.SetViewport(0, 0, (int)screenSize.X, (int)screenSize.Y);
        }

        public void RenderDownsamples(Texture inputTexture)
        {
            downsampleShader.Use();
            downsampleShader.SetVector2("srcResolution", screenSize);

            if (karIsOnDownsample)
            {
                downsampleShader.SetInt("mipLevel", 0);
            }

            downsampleShader.SetInt("INPUT_TEXTURE", 0);
            inputTexture.SetActiveUnit(TextureUnit.Texture0);
            inputTexture.Bind();

            for (int i = 0; i < bloomMipCount; i++)
            {
                BloomMip bloomMip = bloomMipList[i];

                GraphicsState.SetViewport(0, 0, bloomMip.size.X, bloomMip.size.Y);
                bloomFB.SetFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, bloomMip.texture);
                downsampleShader.Apply();
                Graphics.DrawFullScreenTri();
                downsampleShader.SetVector2("srcResolution", bloomMip.size);
                bloomMip.texture.Bind();
                if (i == 0) { downsampleShader.SetInt("mipLevel", 1); }
            }
        }

        public void RenderUpsamples()
        {
            upsampleShader.Use();
            upsampleShader.SetFloat("filterRadius", filterRadius);
            GraphicsState.SetBlending(true);
            GraphicsState.SetBlendingFunc(BlendingFactor.One, BlendingFactor.One);
            GraphicsState.SetBlendingEquation(BlendEquationMode.FuncAdd);

            for (int i = bloomMipCount - 1; i > 0; i--)
            {
                BloomMip bloomMip = bloomMipList[i];
                BloomMip nextMip = bloomMipList[i - 1];

                bloomMip.texture.SetActiveUnit(TextureUnit.Texture0);
                bloomMip.texture.Bind();

                GraphicsState.SetViewport(0, 0, nextMip.size.X, nextMip.size.Y);
                bloomFB.SetFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, nextMip.texture);

                Graphics.DrawFullScreenTri();
            }
        }

        public Framebuffer GetOutputFramebuffer()
        {
            return bloomFB;
        }

        public void Resize(int width, int height)
        {
            screenSize = new Vector2(width, height);


            Vector2 currentMipSize = new Vector2(width, height);

            for (int i = 0; i < bloomMipCount; i++)
            {
                BloomMip bloomMip = bloomMipList[i];

                currentMipSize *= 0.5f;
                if (currentMipSize.X <= 1 || currentMipSize.Y <= 1)
                {
                    continue;
                }

                bloomMip.size = (Vector2i)currentMipSize;

                Texture mipTex = bloomMip.texture;
                mipTex.TexImage2D(bloomMip.size.X, bloomMip.size.Y, PixelInternalFormat.Rgb16f, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
                mipTex.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
                mipTex.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

                bloomMipList[i] = bloomMip;
            }
        }

    }
}
