using EmberaEngine.Engine.Core;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{
    class VolumetricFogPass : IRenderPass
    {
        bool isActive = true;

        Texture volumeTexture;
        Texture emissiveTexture;

        Vector3i VolumeTextureDimensions = new Vector3i(150, 80, 64);

        ComputeShader fogInjectionShader;
        ComputeShader lightInjectionShader;

        Vector2 size;

        public void Initialize(int width, int height)
        {
            size = new Vector2(width, height);

            volumeTexture = new Texture(TextureTarget3d.Texture3D);
            volumeTexture.TexImage3D(VolumeTextureDimensions.X, VolumeTextureDimensions.Y, VolumeTextureDimensions.Z, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            volumeTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            volumeTexture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

            emissiveTexture = new Texture(TextureTarget3d.Texture3D);
            emissiveTexture.TexImage3D(VolumeTextureDimensions.X, VolumeTextureDimensions.Y, VolumeTextureDimensions.Z, PixelInternalFormat.R11fG11fB10f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            emissiveTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            emissiveTexture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

            fogInjectionShader = new ComputeShader("Engine/Content/Shaders/3D/Volumetrics/fogInjection.comp");
            lightInjectionShader = new ComputeShader("Engine/Content/Shaders/3D/Volumetrics/lightInjection.comp");

        }

        void FogInjectionStep()
        {
            fogInjectionShader.Use();
            volumeTexture.BindImageTexture(4, OpenTK.Graphics.OpenGL.TextureAccess.WriteOnly, SizedInternalFormat.Rgba16f);
            emissiveTexture.BindImageTexture(5, OpenTK.Graphics.OpenGL.TextureAccess.WriteOnly, SizedInternalFormat.R11fG11fB10f);
            fogInjectionShader.Dispatch(VolumeTextureDimensions.X / 8, VolumeTextureDimensions.Y / 8, volumeTexture.Depth / 4);

        }

        void LightInjectionStep()
        {
            lightInjectionShader.Use();
            
        }

        public void Apply(FrameData frameData)
        {
            FogInjectionStep();
            LightInjectionStep();
        }

        public Framebuffer GetOutputFramebuffer()
        {
            throw new NotImplementedException();
        }

        public void Resize(int width, int height)
        {
            size = new Vector2(width, height);
        }

        public bool GetState() => isActive;
        public void SetState(bool value) => isActive = value;
    }
}
