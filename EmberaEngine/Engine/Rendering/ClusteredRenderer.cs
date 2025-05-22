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
        public uint count;
        public fixed int lightIndices[100];

        private fixed byte _padding[12];
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
        int numClusters = 12 * 12 * 24;

        Cluster[] clusters;
        LightGrid[] lightGrids;

        uint[] globalLightIndexList;

        List<PointLight> pointLights;
        LightData lightData;

        BufferObject<Cluster> clusterBuffer;
        BufferObject<LightGrid> lightGridBuffer;
        BufferObject<GlobalIndexCount> globalIndexCount;
        BufferObject<uint> globalLightIndexListSSBO;
        BufferObject<PointLight> pointLightSSBO;
        BufferObject<LightData> lightDataUBO;
        BufferObject<ScreenViewData> screenViewDataSSBO;


        private ComputeShader clusterCompute;
        private ComputeShader clusterLightCull;


        private Shader clusteredPBRShader;

        private float oldFOV;

        private uint sizeX;

        // REMOVE THIS

        static Mesh cube = Graphics.GetCube();

        public void Initialize()
        {
            Console.WriteLine("Clustered Renderer has been initialized");

            
            clusterCompute = new ComputeShader("Engine/Content/Shaders/3D/ClusterCompute/cluster.comp", gridSize);
            clusterLightCull = new ComputeShader("Engine/Content/Shaders/3D/ClusterCompute/cluster_light_cull.comp");
            clusteredPBRShader = new Shader("Engine/Content/Shaders/3D/PBR/clustered_pbr");

            numClusters = gridSize.X * gridSize.Y * gridSize.Z;

            clusters = new Cluster[numClusters];
            lightGrids = new LightGrid[numClusters * 100];

            globalLightIndexList = new uint[RenderGraph.MAX_POINT_LIGHTS];

            pointLights = new List<PointLight>();

            // Test light
            for (int i = 0; i < 1; i++)
            {
                pointLights.Add(new PointLight()
                {
                    color = new Vector4(1, 0.5f, 1, 1),
                    enabled = true,
                    position = new Vector4(6 + i, 1f, 13, 1),
                    intensity = 100,
                    range = 10f
                });
            }

            clusterBuffer = new BufferObject<Cluster>(BufferStorageTarget.ShaderStorageBuffer, clusters);
            lightGridBuffer = new BufferObject<LightGrid>(BufferStorageTarget.ShaderStorageBuffer, lightGrids);
            globalIndexCount = new BufferObject<GlobalIndexCount>(BufferStorageTarget.ShaderStorageBuffer, new GlobalIndexCount() { globalLightIndexCount = 0 });
            globalLightIndexListSSBO = new BufferObject<uint>(BufferStorageTarget.ShaderStorageBuffer, globalLightIndexList);
            pointLightSSBO = new BufferObject<PointLight>(BufferStorageTarget.ShaderStorageBuffer, pointLights.ToArray(), BufferUsageHint.DynamicCopy);

            pointLightSSBO.Bind(0);
            lightGridBuffer.Bind(1);
            clusterBuffer.Bind(2);
            globalIndexCount.Bind(3);
            globalLightIndexListSSBO.Bind(4);

        }

        public void BeginRender()
        {
            //Camera camera = Renderer3D.GetRenderCamera();
            //PointLight light = pointLights[0];
            //light.position.X += 0.01f;

            //pointLights[0] = light;

            //unsafe
            //{
            //    pointLightSSBO.SetData(0, pointLights.Count * sizeof(PointLight), pointLights.ToArray());
            //}
        }

        public void Render()
        {
            Camera camera = Renderer3D.GetRenderCamera();

            if (camera.fovy != oldFOV)
            {
                CreateScreenViewSSBO(camera);
                oldFOV = camera.fovy;
                ComputeClusters(camera.nearClip, camera.farClip, camera.GetProjectionMatrix());
            }

            //ComputeClusters(camera.nearClip, camera.farClip, camera.GetProjectionMatrix());
            CullLights(camera.GetViewMatrix());

            clusteredPBRShader.Use();

            clusteredPBRShader.SetVector3("C_VIEWPOS", camera.position);
            clusteredPBRShader.SetVector3("material.albedo", new Vector3(1f, 0.8f, 0.2f));
            clusteredPBRShader.SetFloat("material.roughness", 1f);
            clusteredPBRShader.SetFloat("material.metallic", 0f);

            // MODEL MATRIX
            Matrix4 model = Matrix4.CreateTranslation(0, 0, 10);
            model *= Matrix4.CreateScale(1, 1, 1);
            model *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(0));
            model *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(0));
            model *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(0));

            clusteredPBRShader.SetFloat("zNear", camera.nearClip);
            clusteredPBRShader.SetFloat("zFar", camera.farClip);
            clusteredPBRShader.SetMatrix4("W_VIEW_MATRIX", camera.GetViewMatrix());
            clusteredPBRShader.SetMatrix4("W_PROJECTION_MATRIX", camera.GetProjectionMatrix());
            clusteredPBRShader.SetMatrix4("W_MODEL_MATRIX", model);

            clusteredPBRShader.Apply();

            pointLightSSBO.Bind(0);
            lightGridBuffer.Bind(1);
            clusterBuffer.Bind(2);
            globalIndexCount.Bind(3);
            globalLightIndexListSSBO.Bind(4);
            screenViewDataSSBO.Bind(5);

            cube.Draw();

        }

        public void EndRender()
        {

        }

        public void CreateScreenViewSSBO(Camera camera)
        {
            sizeX = (uint)MathHelper.Ceiling(Screen.Size.X / (float)gridSize.X);
            Console.WriteLine(Screen.Size.X);

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
            screenViewDataSSBO.Bind(5);
        }

        public void CullLights(Matrix4 viewMatrix)
        {
            clusterLightCull.Use();

            clusterLightCull.SetMatrix4("W_VIEW_MATRIX", viewMatrix);

            clusterLightCull.Dispatch(1, 1, 6);
        }

        public void ComputeClusters(float nearClip, float farClip, Matrix4 projectionMatrix)
        {
            clusterCompute.Use();

            clusterCompute.SetFloat("zNear", nearClip);
            clusterCompute.SetFloat("zFar", farClip);

            clusterCompute.Dispatch();
            clusterCompute.Wait();
        }

        public void Resize(int width, int height)
        {

        }
    }
}
