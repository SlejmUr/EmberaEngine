using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{
    internal class SSAOPass : IRenderPass
    {
        Framebuffer SSAOFB;
        Framebuffer SSAOBlurFB;

        Texture SSAOTexture;
        Texture SSAOBlurTexture;
        Texture NoiseTexture;

        Shader SSAOShader;
        Shader SSAOBlurShader;

        int SampleKernelSize = 64;
        Vector3[] SampleKernelValues;

        Vector2 screenDimensions;

        bool isActive = true;

        float renderScale = 0.25f;

        public bool GetState() => isActive;
        public void SetState(bool value) => isActive = value;

        public void Initialize(int width, int height)
        {
            screenDimensions = new Vector2(width * renderScale, height * renderScale);

            Vector3[] RandomizedNoise = Helper.GenerateNoise(16);
            SampleKernelValues = GenerateKernel(SampleKernelSize).ToArray();

            NoiseTexture = new Texture(TextureTarget2d.Texture2D);
            NoiseTexture.TexImage2D(4, 4, PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float, RandomizedNoise);
            NoiseTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            NoiseTexture.SetWrapMode(TextureWrapMode.Repeat, TextureWrapMode.Repeat);
            NoiseTexture.GenerateMipmap();

            SSAOTexture = new Texture();
            SSAOTexture.TexImage2D((int)screenDimensions.X, (int)screenDimensions.Y, PixelInternalFormat.R16f, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            SSAOTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);

            SSAOBlurTexture = new Texture();
            SSAOBlurTexture.TexImage2D((int)screenDimensions.X, (int)screenDimensions.Y, PixelInternalFormat.R16f, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            SSAOBlurTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);

            SSAOFB = new Framebuffer("SSAO Framebuffer");
            SSAOFB.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, SSAOTexture);
            SSAOFB.SetDrawBuffers([OpenTK.Graphics.OpenGL.DrawBuffersEnum.ColorAttachment0]);

            SSAOBlurFB = new Framebuffer("SSAO Blur Framebuffer");
            SSAOBlurFB.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, SSAOBlurTexture);
            SSAOBlurFB.SetDrawBuffers([OpenTK.Graphics.OpenGL.DrawBuffersEnum.ColorAttachment0]);



            SSAOShader = new Shader("Engine/Content/Shaders/3D/AO/ssao");

            SSAOShader.Set("gPosition", 0);
            SSAOShader.Set("gNormal", 1);
            SSAOShader.Set("texNoise", 2);
            SSAOShader.Set("gDepth", 3);
            SSAOShader.Set("screenDimensions", screenDimensions);

            SSAOBlurShader = new Shader("Engine/Content/Shaders/3D/AO/ssao.vert", "Engine/Content/Shaders/3D/AO/ssaoBlur.frag");
            SSAOBlurShader.Set("INPUT_TEXTURE", 0);

            for (int i = 0; i < SampleKernelSize; i++)
            {
                SSAOShader.Set("samples[" + i + "]", SampleKernelValues[i]);
            }
        }

        // Ensure Geometry pass is run before this pass as it makes use of normal texture & position texture
        public void Apply(FrameData frameData)
        {
            if (!isActive) return;
            SSAOFB.Bind();
            GraphicsState.SetViewport(0, 0, (int)screenDimensions.X, (int)screenDimensions.Y);
            GraphicsState.Clear(true, true);
            GraphicsState.SetCulling(false);
            GraphicsState.SetBlending(false);

            SSAOShader.Use();
            SSAOShader.SetMatrix4("W_PROJECTION_MATRIX", frameData.Camera.GetProjectionMatrix());
            SSAOShader.SetMatrix4("W_INVERSE_VIEW_MATRIX", Matrix4.Invert(frameData.Camera.GetViewMatrix()));
            SSAOShader.SetMatrix4("W_VIEW_MATRIX", frameData.Camera.GetViewMatrix());

            Texture positionTexture = frameData.GBuffer.GetFramebufferTexture(1);
            Texture normalTexture = frameData.GBuffer.GetFramebufferTexture(0);

            positionTexture.SetActiveUnit(TextureUnit.Texture0);
            positionTexture.Bind();

            normalTexture.SetActiveUnit(TextureUnit.Texture1);
            normalTexture.Bind();

            NoiseTexture.SetActiveUnit(TextureUnit.Texture2);
            NoiseTexture.Bind();

            SSAOShader.Apply();
            Graphics.DrawFullScreenTri();

            SSAOBlurFB.Bind();
            GraphicsState.SetViewport(0, 0, (int)screenDimensions.X, (int)screenDimensions.Y);
            GraphicsState.Clear(true, true);

            SSAOBlurShader.Use();

            SSAOTexture.SetActiveUnit(TextureUnit.Texture0);
            SSAOTexture.Bind();

            SSAOBlurShader.Apply();

            Graphics.DrawFullScreenTri();

        }

        static float ourLerp(float a, float b, float f)
        {
            return a + f * (b - a);
        }

        public static List<Vector3> GenerateKernel(int kernelSize = 64)
        {
            var ssaoKernel = new List<Vector3>();
            var random = new Random();

            for (int i = 0; i < kernelSize; ++i)
            {
                Vector3 sample = new Vector3(
                    (float)(random.NextDouble() * 2.0 - 1.0),
                    (float)(random.NextDouble() * 2.0 - 1.0),
                    (float)(random.NextDouble())
                );

                sample = Vector3.Normalize(sample);
                sample *= (float)random.NextDouble();

                float scale = (float)i / kernelSize;
                scale = ourLerp(0.1f, 1.0f, scale * scale);
                sample *= scale;

                ssaoKernel.Add(sample);
            }

            return ssaoKernel;
        }

        public Framebuffer GetOutputFramebuffer()
        {
            return SSAOBlurFB;
        }

        public void Resize(int width, int height)
        {
            screenDimensions = new Vector2(width * renderScale, height * renderScale);
            SSAOShader.Set("screenDimensions", screenDimensions);

            SSAOTexture.TexImage2D((int)screenDimensions.X, (int)screenDimensions.Y, PixelInternalFormat.R16f, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            SSAOTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);

            SSAOBlurTexture.TexImage2D((int)screenDimensions.X, (int)screenDimensions.Y, PixelInternalFormat.R16f, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            SSAOBlurTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        }
    }
}
