using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmberaEngine.Engine.Rendering;
using Newtonsoft.Json.Linq;

namespace EmberaEngine.Engine.Rendering
{
    class Render2DQueueElement
    {
        public Vector4 dimensions;
        public float rotation;
        public List<Texture> textures;
        public Shader shader;
        public Color4 color;
        public Mesh mesh;
        


        public Render2DQueueElement()
        {
            textures = new List<Texture>();
        }
        
    }


    public class Renderer2D
    {

        static Shader Basic2DShader;
        static Shader Font2DShader;
        static Shader NineSliceShader;

        public static Matrix4 Projection;
        static Mesh PlaneMesh;

        // Framebuffer Textures
        static Texture CompositeBufferTexture;

        // Framebuffers
        static Framebuffer CompositeBuffer2D;

        //Queues
        static List<Render2DQueueElement> RenderQueue;

        public static void Initialize(int width, int height)
        {
            Basic2DShader = new Shader("Engine/Content/Shaders/2D/sprite2d");
            Font2DShader = new Shader("Engine/Content/Shaders/2D/font");
            NineSliceShader = new Shader("./Engine/Content/Shaders/2D/sprite2d.vert", "./Engine/Content/Shaders/2D/nine_slice.frag", "");

            Projection = Graphics.CreateOrthographic2D(width, height, -1f, 1f);

            Vertex[] vertices = Primitives.GetPlaneVertices();

            VertexBuffer vertexBuffer = new VertexBuffer(Vertex.VertexInfo, vertices.Length);
            vertexBuffer.SetData(vertices, vertices.Length);
            VertexArray PlaneVAO = new VertexArray(vertexBuffer);
            PlaneMesh = new Mesh();
            PlaneMesh.SetVertexArrayObject(PlaneVAO);

            RenderQueue = new List<Render2DQueueElement>();


            // Setting up composite buffer
            CompositeBufferTexture = new Texture(TextureTarget2d.Texture2D);
            CompositeBufferTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            CompositeBufferTexture.GenerateMipmap();

            CompositeBuffer2D = new Framebuffer("COMPOSITE FB");
            CompositeBuffer2D.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, CompositeBufferTexture);
        }

        public static void BeginRender()
        {
            CompositeBuffer2D.Bind();
            GraphicsState.ClearColor(0, 0, 0, 1);
            GraphicsState.Clear(true, true);
            GraphicsState.SetViewport(0, 0, Renderer.Width, Renderer.Height);
            GraphicsState.SetCulling(true);
            GraphicsState.SetDepthTest(true);
            GraphicsState.SetBlending(true);
            GraphicsState.SetBlendingFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        public static void Render()
        {
            CanvasRenderer.Render();

            int currentlyBoundShader = 0;
            for (int i = 0; i < RenderQueue.Count; i++)
            {
                Render2DQueueElement elem = RenderQueue[i];

                if (currentlyBoundShader != elem.shader.GetRendererID()) 
                    elem.shader.Use();
                currentlyBoundShader = elem.shader.GetRendererID();


                Matrix4 model = Matrix4.CreateScale(-elem.dimensions.Z / 2, elem.dimensions.W / 2, 1f);
                model *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(elem.rotation + 180));
                model *= Matrix4.CreateTranslation(elem.dimensions.X + elem.dimensions.Z / 2, elem.dimensions.Y + elem.dimensions.W / 2, 0f);

                elem.shader.SetMatrix4("W_MODEL_MATRIX", model);
                elem.shader.SetMatrix4("W_PROJECTION_MATRIX", Projection);

                elem.shader.SetInt("u_Texture", 0);
                elem.shader.SetVector4("u_Color", Helper.ToVector4(elem.color));

                for (int j = 0; j < elem.textures.Count; j++)
                {
                    elem.textures[j].SetActiveUnit(TextureUnit.Texture0 + j);
                    elem.textures[j].Bind();
                }


                elem.mesh.Draw();
                //elem.shader.Clear();
            }

            RenderQueue.Clear();
        }

        public static void EndRender()
        {
            GraphicsState.ClearTextureBinding2D();
            GraphicsState.ClearFrameBufferBinding();
        }

        public static void RenderTexturedRect(Texture tex, Vector4 dimensions, float rotation = 0)
        {
            RenderQueue.Add(new Render2DQueueElement()
            {
                textures = new List<Texture> { tex },
                dimensions = dimensions,
                shader = Basic2DShader,
                mesh = PlaneMesh,
            });
        }

        public static void RenderCustomShaderRect(Texture tex, Vector4 dimensions, Shader shader, float rotation = 0)
        {
            RenderQueue.Add(new Render2DQueueElement()
            {
                textures = new List<Texture> { tex },
                dimensions = dimensions,
                shader = shader,
                mesh = PlaneMesh,
            });
        }

        public static void RenderNineSliceRect(Color4 color, Vector4 dimensions)
        {
            RenderQueue.Add(new Render2DQueueElement()
            {
                color = color,
                dimensions = dimensions,
                shader = NineSliceShader,
                mesh = PlaneMesh,
            });
        }

        public static void RenderColorRect(Color4 color, Vector4 dimensions)
        {
            RenderQueue.Add(new Render2DQueueElement()
            {
                color = color,
                dimensions = dimensions,
                shader = Basic2DShader,
                mesh = PlaneMesh,
            });
        }

        public static void Resize(int width, int height)
        {
            CompositeBufferTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            CompositeBufferTexture.GenerateMipmap();
            Projection = Graphics.CreateOrthographic2D(width, height, -1f, 1f);
        }

        public static Framebuffer GetComposite2D()
        {
            return CompositeBuffer2D;
        }


    }
}
