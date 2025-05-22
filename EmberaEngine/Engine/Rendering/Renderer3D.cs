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

    public interface IRenderPipeline
    {
        void Initialize();
        void BeginRender();
        void Render();
        void EndRender();

        void Resize(int width, int height);
    }

    public class Renderer3D
    {
        public static List<Camera> cameras;
        public static IRenderPipeline ActiveRenderingPipeline;

        static Texture CompositeBufferTexture;

        static Texture NormalBufferTexture;
        static Texture DepthBufferTexture;
        static Texture GeometryBufferTexture;

        static Framebuffer CompositeBuffer;
        static Framebuffer GeometryBuffer;

        // REMOVE
        static Mesh cube = Graphics.GetCube();
        static Shader shader = new("Engine/Content/Shaders/3D/basic/base");

        public static void Initialize(int width, int height)
        {
            cameras = new List<Camera>();

            // Setting up composite buffer
            CompositeBufferTexture = new Texture(TextureTarget2d.Texture2D);
            CompositeBufferTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            CompositeBufferTexture.GenerateMipmap();

            CompositeBuffer = new Framebuffer("Renderer3D_Composite_Framebuffer");
            CompositeBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, CompositeBufferTexture);

            ModelRenderer.Initialize();
            
            ActiveRenderingPipeline.Initialize();
        }

        public static void RegisterCamera(Camera camera)
        {
            cameras.Add(camera);
            camera.rendererID = cameras.Count;
        }

        public static void RemoveCamera(Camera camera)
        {
            cameras.Remove(camera);
        }

        public static Camera GetRenderCamera()
        {
            for (int i = 0; i < cameras.Count; i++)
            {
                if (cameras[i].isDefault)
                {
                    return cameras[i];
                }
            }

            return null;
        }

        public static void BeginRender()
        {
            Camera camera = GetRenderCamera();
            if (camera == null) return;

            CompositeBuffer.Bind();
            GraphicsState.ClearColor(camera.ClearColor);
            GraphicsState.Clear(true, true);
            GraphicsState.SetViewport(0, 0, Renderer.Width, Renderer.Height);
            GraphicsState.SetCulling(true);
            GraphicsState.SetDepthTest(true);
            GraphicsState.SetBlending(true);
            GraphicsState.SetBlendingFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

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

        public static void Resize(int width, int height)
        {
            Console.WriteLine(width + " " + height);
            CompositeBufferTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            CompositeBufferTexture.GenerateMipmap();
            Console.WriteLine("RESIZED");

            ActiveRenderingPipeline.Resize(width, height);
        }

        public static Framebuffer GetComposite()
        {
            return CompositeBuffer;
        }
    }
}
