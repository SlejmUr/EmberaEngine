using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace EmberaEngine.Engine.Rendering
{
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    unsafe struct Cluster
    {
        public Vector4 minPoint;
        public Vector4 maxPoint;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    unsafe struct LightGrid
    {
        public uint offset;
        public uint count;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct GlobalIndexCount
    {
        public uint globalLightIndexCount;
    }

    [StructLayout(LayoutKind.Explicit, Size = 96)]
    struct ScreenViewData
    {
        [FieldOffset(0)]
        public Matrix4 inverseProjectionMatrix; // 64 bytes

        [FieldOffset(64)]
        public Vector4i tileSizes; // 16 bytes

        [FieldOffset(80)]
        public uint screenWidth;

        [FieldOffset(84)]
        public uint screenHeight;

        [FieldOffset(88)]
        public float sliceScaling;

        [FieldOffset(92)]
        public float sliceBias;
    }


    class ClusteredRenderer : IRenderPipeline
    {
        Vector3i gridSize = new Vector3i(16, 9, 24);
        int numClusters;

        Cluster[] clusters;
        LightGrid[] lightGrids;
        uint[] globalLightIndexList;


        BufferObject<Cluster> clusterBuffer;
        BufferObject<LightGrid> lightGridBuffer;
        BufferObject<GlobalIndexCount> globalIndexCount;
        BufferObject<uint> globalLightIndexListSSBO;
        BufferObject<LightData> lightDataUBO;
        BufferObject<ScreenViewData> screenViewDataSSBO;



        Framebuffer TonemappedBuffer;

        Texture TonemappedTexture;
        Texture TonemappedDepthTexture;

        private ComputeShader clusterCompute;
        private ComputeShader clusterLightCull;

        private IRenderPass GBufferPass;
        private IRenderPass SSAOPass;
        private IRenderPass VolumetricFogPass;
        private IRenderPass FXAAPass;
        private IRenderPass BloomPass;
        

        private Shader clusteredPBRShader;
        private Shader fullScreenTonemap;

        private RenderSetting renderSettings = new RenderSetting()
        {
            useBloom = true,
            useSSAO = true,
            tonemapFunction = TonemapFunction.ACES,
            Exposure = 1f,
            AmbientColor = new Color4(0.1f, 0.1f, 0.1f, 0.1f),
            useSkybox = true,
            AmbientFactor = 0.1f,
            useIBL = true,
            useShadows = true,
            useAntialiasing = true,
        };

        private float oldFOV;

        private uint sizeX;

        private uint numLightsPerTile = 100;

        // REMOVE THIS

        static Mesh cube = Graphics.GetCube();

        public void Initialize(int width, int height)
        {


            Console.WriteLine("Clustered Renderer initializing.");

            
            clusterCompute = new ComputeShader("Engine/Content/Shaders/3D/ClusterCompute/cluster.comp", gridSize);
            clusterLightCull = new ComputeShader("Engine/Content/Shaders/3D/ClusterCompute/cluster_light_cull.comp");
            clusteredPBRShader = new Shader("Engine/Content/Shaders/3D/PBR/clustered_pbr");
            fullScreenTonemap = new Shader("Engine/Content/Shaders/3D/basic/tonemap");

            numClusters = gridSize.X * gridSize.Y * gridSize.Z;
            clusters = new Cluster[numClusters];
            lightGrids = new LightGrid[numClusters * 100];
            globalLightIndexList = new uint[numLightsPerTile * numClusters];

            clusterBuffer = new BufferObject<Cluster>(BufferStorageTarget.ShaderStorageBuffer, clusters);
            lightGridBuffer = new BufferObject<LightGrid>(BufferStorageTarget.ShaderStorageBuffer, lightGrids);
            globalIndexCount = new BufferObject<GlobalIndexCount>(BufferStorageTarget.ShaderStorageBuffer, new GlobalIndexCount() { globalLightIndexCount = 0 });
            globalLightIndexListSSBO = new BufferObject<uint>(BufferStorageTarget.ShaderStorageBuffer, globalLightIndexList);
            
            TonemappedTexture = new Texture(Core.TextureTarget2d.Texture2D);
            TonemappedTexture.TexImage2D(width, height, Core.PixelInternalFormat.Rgba16f, Core.PixelFormat.Rgba, Core.PixelType.Float, IntPtr.Zero);
            TonemappedTexture.SetFilter(Core.TextureMinFilter.Linear, Core.TextureMagFilter.Linear);

            TonemappedDepthTexture = new Texture(Core.TextureTarget2d.Texture2D);
            TonemappedDepthTexture.TexImage2D(width, height, Core.PixelInternalFormat.Depth24Stencil8, Core.PixelFormat.DepthComponent, Core.PixelType.Float, IntPtr.Zero);
            TonemappedDepthTexture.SetFilter(Core.TextureMinFilter.Nearest, Core.TextureMagFilter.Nearest);

            TonemappedBuffer = new Framebuffer("Tonemap FrameBuffer");
            TonemappedBuffer.AttachFramebufferTexture(FramebufferAttachment.ColorAttachment0, TonemappedTexture);
            TonemappedBuffer.AttachFramebufferTexture(FramebufferAttachment.DepthStencilAttachment, TonemappedDepthTexture);
            TonemappedBuffer.SetDrawBuffers([DrawBuffersEnum.ColorAttachment0]);

            GBufferPass = new GBufferPass();
            GBufferPass.Initialize(width, height);

            SSAOPass = new SSAOPass();
            SSAOPass.Initialize(width, height);

            VolumetricFogPass = new VolumetricFogPass();
            VolumetricFogPass.Initialize(width, height);

            FXAAPass = new AntiAliasPass();
            FXAAPass.Initialize(width, height);

            BloomPass = new BloomPass();
            BloomPass.Initialize(width, height);

            LightManager.GetLightSSBO().Bind((int)RenderGraph.SSBOBindIndex.LightBuffer);
            lightGridBuffer.Bind((int)RenderGraph.SSBOBindIndex.LightGridBuffer);
            clusterBuffer.Bind((int)RenderGraph.SSBOBindIndex.ClusterBuffer);
            globalIndexCount.Bind((int)RenderGraph.SSBOBindIndex.GlobalLightIndexCount);
            globalLightIndexListSSBO.Bind((int)RenderGraph.SSBOBindIndex.GlobalLightIndexList);

            SkyboxManager.Initialize();

            Console.WriteLine("Clustered Renderer initialized.");
        }

        public void BeginRender()
        {
            Camera camera = Renderer3D.GetRenderCamera();

            FrameData frameData = Renderer3D.GetFrameData();

            GBufferPass.Apply(frameData);
            SSAOPass.Apply(frameData);

            if (renderSettings.useSkybox)
                SkyboxManager.Render();

            Renderer3D.GetComposite().Bind();
            Renderer3D.ApplyPerFrameSettings(camera);
        }

        public void Render()
        {
            Camera camera = Renderer3D.GetRenderCamera();

            if (camera.fovy != oldFOV)
            {
                Console.WriteLine("Called");
                CreateScreenViewSSBO(camera);
                oldFOV = camera.fovy;
                ComputeClusters(camera.nearClip, camera.farClip, camera.GetProjectionMatrix());
            }

            CullLights(camera.GetViewMatrix());

            List<Mesh> meshes = Renderer3D.GetMeshes();

            if (renderSettings.useSkybox)
                SkyboxManager.RenderCube();

            for (int i = 0; i < meshes.Count; i++)
            {
                Mesh mesh = meshes[i];
                Material material = MaterialManager.GetMaterial(mesh.MaterialIndex);

                //if (i == 0)
                //{
                //    material.shader.Use();
                //}

                material.shader.Use();
                material.Apply();

                int textureStartIndex = 5;

                material.shader.SetInt("irradianceMap", textureStartIndex);
                material.shader.SetInt("prefilterMap", textureStartIndex + 1);
                material.shader.SetInt("brdfLUT", textureStartIndex + 2);

                GraphicsState.SetTextureActiveBinding(Core.TextureUnit.Texture0 + textureStartIndex);
                SkyboxManager.GetIrradianceMap().Bind();

                GraphicsState.SetTextureActiveBinding(Core.TextureUnit.Texture0 + textureStartIndex + 1);
                SkyboxManager.GetPreFilterMap().Bind();

                GraphicsState.SetTextureActiveBinding(Core.TextureUnit.Texture0 + textureStartIndex + 2);
                SkyboxManager.GetBRDFLUT().Bind();

                Matrix4 model = mesh.worldMatrix;


                material.shader.SetVector3("ambientColor", new Vector3(renderSettings.AmbientColor.R, renderSettings.AmbientColor.G, renderSettings.AmbientColor.B));
                material.shader.SetFloat("ambientFactor", renderSettings.AmbientFactor);
                material.shader.SetBool("useIBL", renderSettings.useIBL);
                material.shader.SetFloat("zFar", camera.farClip);
                material.shader.SetFloat("zNear", camera.nearClip);
                material.shader.SetVector3("C_VIEWPOS", camera.position);
                material.shader.SetMatrix4("W_MODEL_MATRIX", model);
                material.shader.SetMatrix4("W_VIEW_MATRIX", camera.GetViewMatrix());
                material.shader.SetMatrix4("W_PROJECTION_MATRIX", camera.GetProjectionMatrix());


                material.shader.Apply();

                mesh.Draw();
            }
        }


        public void EndRender()
        {
            FrameData frameData = Renderer3D.GetFrameData();
            frameData.EffectFrameBuffer = Renderer3D.GetComposite();

            FXAAPass.Apply(frameData);
            //VolumetricFogPass.Apply(frameData);
            BloomPass.Apply(frameData);

            CombineEffects();
        }

        public void CombineEffects()
        {
            Framebuffer.BlitFrameBuffer(Renderer3D.GetComposite(), TonemappedBuffer, (0, 0, Renderer.Width, Renderer.Height), (0, 0, Renderer.Width, Renderer.Height), ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);

            TonemappedBuffer.Bind();
            GraphicsState.SetCulling(false);
            GraphicsState.SetViewport(0, 0, Screen.Size.X, Screen.Size.Y);
            GraphicsState.Clear(true);
            GraphicsState.SetDepthTest(false);
            fullScreenTonemap.Use();
            fullScreenTonemap.SetInt("SCREEN_TEXTURE", 0);
            fullScreenTonemap.SetInt("AO_TEXTURE", 1);
            fullScreenTonemap.SetInt("BLOOM_TEXTURE", 2);
            fullScreenTonemap.SetInt("VOLUMETRIC_TEXTURE", 3);
            fullScreenTonemap.SetInt("TONEMAP_FUNCTION", (int)renderSettings.tonemapFunction);
            fullScreenTonemap.SetInt("USE_AO", renderSettings.useSSAO ? 1 : 0);
            fullScreenTonemap.SetInt("USE_BLOOM", renderSettings.useBloom ? 1 : 0);
            fullScreenTonemap.SetFloat("EXPOSURE", renderSettings.Exposure);
            GraphicsState.SetTextureActiveBinding(Core.TextureUnit.Texture0);
            FXAAPass.GetOutputFramebuffer().GetFramebufferTexture(0).Bind();
            //Renderer3D.GetComposite().GetFramebufferTexture(0).Bind();
            GraphicsState.SetTextureActiveBinding(Core.TextureUnit.Texture1);
            SSAOPass.GetOutputFramebuffer().GetFramebufferTexture(0).Bind();
            GraphicsState.SetTextureActiveBinding(Core.TextureUnit.Texture2);
            BloomPass.GetOutputFramebuffer().GetFramebufferTexture(0).Bind();
            //GraphicsState.SetTextureActiveBinding(Core.TextureUnit.Texture3);
            //VolumetricFogPass.GetOutputFramebuffer().GetFramebufferTexture(0).Bind();
            fullScreenTonemap.Apply();
            Graphics.DrawFullScreenTri();
            GraphicsState.SetDepthTest(true);
        }


        public void CreateScreenViewSSBO(Camera camera)
        {
            sizeX = (uint)MathHelper.Ceiling(Screen.Size.X / (float)gridSize.X);

            // This is generated here since the camera does not get assigned at the initialize stage.
            ScreenViewData screenViewData = new ScreenViewData();
            screenViewData.screenWidth = (uint)Screen.Size.X;
            screenViewData.screenHeight = (uint)Screen.Size.Y;
            screenViewData.sliceScaling = (float)gridSize.Z / (float)Math.Log(camera.farClip / camera.nearClip, 2);
            screenViewData.sliceBias = -(gridSize.Z * (float)Math.Log(camera.nearClip, 2) / (float)Math.Log(camera.farClip / camera.nearClip, 2));
            screenViewData.tileSizes.X = gridSize.X;
            screenViewData.tileSizes.Y = gridSize.Y;
            screenViewData.tileSizes.Z = gridSize.Z;
            screenViewData.tileSizes.W = (int)sizeX;
            screenViewData.inverseProjectionMatrix = Matrix4.Invert(camera.GetProjectionMatrix());
            if (screenViewDataSSBO != null)
            {
                GraphicsObjectCollector.AddBufferToDispose(screenViewDataSSBO.GetRendererID());
            }
            screenViewDataSSBO = new BufferObject<ScreenViewData>(BufferStorageTarget.ShaderStorageBuffer, screenViewData);
            screenViewDataSSBO.Bind((int)RenderGraph.SSBOBindIndex.ScreenInfoBuffer);
        }

        public void CullLights(Matrix4 viewMatrix)
        {
            clusterLightCull.Use();

            clusterLightCull.SetMatrix4("W_VIEW_MATRIX", viewMatrix);

            clusterLightCull.Dispatch(1, 1, 6);
            clusterLightCull.Wait();
        }

        public void ComputeClusters(float nearClip, float farClip, Matrix4 projectionMatrix)
        {
            clusterCompute.Use();

            clusterCompute.SetFloat("zNear", nearClip);
            clusterCompute.SetFloat("zFar", farClip);

            clusterCompute.Dispatch();
            clusterCompute.Wait();
        }

        public Framebuffer GetOutputFrameBuffer()
        {
            //return Renderer3D.GetComposite();
            return TonemappedBuffer;
        }

        public Material GetDefaultMaterial()
        {
            Material pbrMaterial = new Material(ShaderRegistry.GetShader("CLUSTERED_PBR"));
            pbrMaterial.Set("material.albedo", Vector4.One);
            pbrMaterial.Set("material.emission", Vector3.Zero);
            pbrMaterial.Set("material.emissionStr", 0);
            pbrMaterial.Set("material.metallic", 0);
            pbrMaterial.Set("material.roughness", 1f);
            pbrMaterial.Set("material.useDiffuseMap", 0);
            pbrMaterial.Set("material.useRoughnessMap", 0);
            pbrMaterial.Set("material.useNormalMap", 0);
            pbrMaterial.Set("material.useEmissionMap", 0);

            pbrMaterial.Set("material.DIFFUSE_TEX", Texture.GetWhite2D());
            pbrMaterial.Set("material.NORMAL_TEX", Texture.GetWhite2D());
            pbrMaterial.Set("material.ROUGHNESS_TEX", Texture.GetWhite2D());
            pbrMaterial.Set("material.EMISSION_TEX", Texture.GetBlack2D());

            return pbrMaterial;
        }

        public void Resize(int width, int height)
        {
            TonemappedTexture.TexImage2D(width, height, Core.PixelInternalFormat.Rgba16f, Core.PixelFormat.Rgba, Core.PixelType.Float, IntPtr.Zero);
            TonemappedTexture.GenerateMipmap();

            TonemappedDepthTexture.TexImage2D(width, height, Core.PixelInternalFormat.Depth24Stencil8, Core.PixelFormat.DepthComponent, Core.PixelType.Float, IntPtr.Zero);
            TonemappedDepthTexture.SetFilter(Core.TextureMinFilter.Nearest, Core.TextureMagFilter.Nearest);

            GBufferPass.Resize(width, height);
            SSAOPass.Resize(width, height);
            BloomPass.Resize(width, height);
            FXAAPass.Resize(width, height);

            Camera camera = Renderer3D.GetRenderCamera();
            if (camera == null) { return; }
            CreateScreenViewSSBO(camera);
            ComputeClusters(camera.nearClip, camera.farClip, camera.GetProjectionMatrix());
        }

        List<IRenderPass> IRenderPipeline.GetPasses()
        {
            return new List<IRenderPass>()
            {
                GBufferPass,
                SSAOPass,
                BloomPass,
                VolumetricFogPass,

            };
        }

        public void SetRenderSettings(RenderSetting renderSetting)
        {
            this.renderSettings = renderSetting;

            BloomPass.SetState(renderSetting.useBloom);
            SSAOPass.SetState(renderSetting.useSSAO);
            FXAAPass.SetState(renderSetting.useAntialiasing);

        }

        public RenderSetting GetRenderSettings()
        {
            return renderSettings;
        }
    }
}
