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
    public enum TonemapFunction
    {
        ACES = 0,
        Filmic = 1,
        Reinhard = 2
    }

    public struct RenderSetting
    {
        public float Exposure;
        public bool useBloom;
        public bool useSSAO;
        public bool useAntialiasing;
        public bool useSkybox;
        public bool useIBL;
        public bool useShadows;
        public TonemapFunction tonemapFunction;
        public Color4 AmbientColor;
        public float AmbientFactor;
    }

    public interface IRenderPipeline
    {
        void Initialize(int width, int height);
        void BeginRender();
        void Render();
        void EndRender();

        void Resize(int width, int height);

        Framebuffer GetOutputFrameBuffer();

        Material GetDefaultMaterial();

        List<IRenderPass> GetPasses();
        RenderSetting GetRenderSettings();
        void SetRenderSettings(RenderSetting settings);
    }

    public interface IRenderPass
    {
        bool GetState();
        void SetState(bool value);

        void Initialize(int width, int height);
        void Resize(int width, int height);
        void Apply(FrameData frameData);
        Framebuffer GetOutputFramebuffer();
    }

    public class FrameData
    {
        public Camera Camera;
        public List<Mesh> Meshes;
        public Framebuffer GBuffer;
        public Framebuffer EffectFrameBuffer; // this is sort of confusing and must be changed to elsewhere or a better system
                                              // its just a way to send to the effect what framebuffer/texture you want as input.
                                              // i implemented this for bloom, as i had no other way to send a input texture.
        public Texture EffectTexture;
    }


    public class Renderer3D
    {

        public static Camera renderCamera;

        public static IRenderPipeline ActiveRenderingPipeline;

        static Texture CompositeBufferTexture;
        static Texture CompositeBufferEmissionTexture;

        static Texture NormalBufferTexture;
        static Texture DepthBufferTexture;
        static Texture GeometryBufferTexture;

        static Framebuffer CompositeBuffer;
        static Framebuffer GeometryBuffer;

        static List<Mesh> meshes;

        static FrameData frameData;

        public static void Initialize(int width, int height)
        {
            //cameras = new List<Camera>();
            meshes = new List<Mesh>();

            // Setting up composite buffer
            CompositeBufferTexture = new Texture(TextureTarget2d.Texture2D);
            CompositeBufferTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            CompositeBufferTexture.GenerateMipmap();

            CompositeBufferEmissionTexture = new Texture(TextureTarget2d.Texture2D);
            CompositeBufferEmissionTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            CompositeBufferEmissionTexture.GenerateMipmap();

            DepthBufferTexture = new Texture(TextureTarget2d.Texture2D);
            DepthBufferTexture.TexImage2D(width, height, PixelInternalFormat.Depth24Stencil8, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            DepthBufferTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);


            CompositeBuffer = new Framebuffer("Renderer3D_Composite_Framebuffer");
            CompositeBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, CompositeBufferTexture);
            CompositeBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment1, CompositeBufferEmissionTexture);
            CompositeBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.DepthStencilAttachment, DepthBufferTexture);
            CompositeBuffer.SetDrawBuffers([OpenTK.Graphics.OpenGL.DrawBuffersEnum.ColorAttachment0, OpenTK.Graphics.OpenGL.DrawBuffersEnum.ColorAttachment1]);

            frameData = new();

            LightManager.Initialize();
            MaterialManager.Initialize();
            
            ActiveRenderingPipeline.Initialize(width, height);
        }

        //public static void RegisterCamera(Camera camera)
        //{
        //    cameras.Add(camera);
        //    camera.rendererID = cameras.Count;
        //}

        //public static void RemoveCamera(Camera camera)
        //{
        //    cameras.Remove(camera);
        //}

        public static void RegisterMesh(Mesh mesh)
        {
            meshes.Add(mesh);
        }

        public static void RemoveMesh(Mesh mesh)
        {
            meshes.Remove(mesh);
        }

        public static FrameData GetFrameData()
        {
            return frameData;
        }

        public static List<Mesh> GetMeshes()
        {
            return meshes;
        }

        public static void SetRenderCamera(Camera camera)
        {
            renderCamera = camera;
            ActiveRenderingPipeline.Resize(Renderer.Width, Renderer.Height);
        }

        public static Camera GetRenderCamera()
        {
            return renderCamera;

            //for (int i = 0; i < cameras.Count; i++)
            //{
            //    if (cameras[i].isDefault)
            //    {
            //        return cameras[i];
            //    }
            //}

            //return null;
        }

        public static void BeginRender()
        {
            Camera camera = GetRenderCamera();
            if (camera == null) return;

            frameData.Camera = camera;
            frameData.Meshes = GetMeshes();

            LightManager.UpdateLights();
            ActiveRenderingPipeline.BeginRender();
        }

        public static void Render()
        {
            Camera camera = GetRenderCamera();
            if (camera == null) return;

            ActiveRenderingPipeline.Render();
        }

        public static void EndRender()
        {
            Camera camera = GetRenderCamera();
            if (camera == null) return;

            ActiveRenderingPipeline.EndRender();
        }

        public static void ApplyPerFrameSettings(Camera camera)
        {
            GraphicsState.ClearColor(camera.ClearColor);
            GraphicsState.Clear(true, true);
            GraphicsState.SetViewport(0, 0, Renderer.Width, Renderer.Height);
            GraphicsState.SetCulling(true);
            GraphicsState.SetDepthTest(true);
            GraphicsState.SetBlending(true);
            GraphicsState.SetBlendingFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        public static void SetViewportDimensions()
        {
            GraphicsState.SetViewport(0, 0, Renderer.Width, Renderer.Height);
        }

        public static void Resize(int width, int height)
        {
            CompositeBufferTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            CompositeBufferTexture.GenerateMipmap();

            CompositeBufferEmissionTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            CompositeBufferEmissionTexture.GenerateMipmap();

            DepthBufferTexture.TexImage2D(width, height, PixelInternalFormat.Depth24Stencil8, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            DepthBufferTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);

            ActiveRenderingPipeline.Resize(width, height);
        }

        public static Framebuffer GetComposite()
        {
            return CompositeBuffer;
        }

        public static Framebuffer GetOutputFrameBuffer()
        {
            return ActiveRenderingPipeline.GetOutputFrameBuffer();
        }
    }
}
