using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmberaEngine.Engine.Utilities;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace EmberaEngine.Engine.Rendering
{
    public static class LightManager
    {
        static List<PointLight> pointLights;
        static List<int> emptyPointLights;

        static List<SpotLight> spotLights;
        static List<int> emptySpotLights;

        static DirectionalLight directionalLight;

        static BufferObject<byte> packedLightSSBO;

        static int headerSize = 16;


        public static void Initialize()
        {
            pointLights = new List<PointLight>();
            emptyPointLights = new List<int>();

            spotLights = new List<SpotLight>();
            emptySpotLights = new List<int>();

            directionalLight = new DirectionalLight();

            uint totalSize =  (uint)(16 + RenderGraph.MAX_POINT_LIGHTS * Marshal.SizeOf<GPUPointLight>() + RenderGraph.MAX_SPOT_LIGHTS * Marshal.SizeOf<GPUSpotLight>() + RenderGraph.MAX_DIR_LIGHTS * Marshal.SizeOf<GPUDirectionalLight>());

            packedLightSSBO = new BufferObject<byte>(BufferStorageTarget.ShaderStorageBuffer, totalSize, BufferUsageHint.DynamicCopy);
        }

        public static BufferObject<byte> GetLightSSBO()
        {
            return packedLightSSBO;
        }

        static int pointLightOffset(int index)
            => headerSize + index * Marshal.SizeOf<GPUPointLight>();

        static int spotLightOffset(int index)
            => headerSize + RenderGraph.MAX_POINT_LIGHTS * Marshal.SizeOf<GPUPointLight>()
                          + index * Marshal.SizeOf<GPUSpotLight>();

        static int directionalLightOffset()
            => headerSize
             + RenderGraph.MAX_POINT_LIGHTS * Marshal.SizeOf<GPUPointLight>()
             + RenderGraph.MAX_SPOT_LIGHTS * Marshal.SizeOf<GPUSpotLight>();

        public static void UpdateLights()
        {
            for (int i = 0; i < pointLights.Count; i++)
            {
                if (pointLights[i].needsUpdate)
                {
                    UpdatePointLight(i);
                    pointLights[i].needsUpdate = false;
                }
            }

            for (int i = 0; i < spotLights.Count; i++)
            {
                if (spotLights[i].needsUpdate)
                {
                    UpdateSpotLight(i);
                    spotLights[i].needsUpdate = false;
                }
            }

            if (directionalLight.needsUpdate)
            {
                UpdateDirectionalLight();
                directionalLight.needsUpdate = false;
            }
        }

        public static PointLight AddPointLight(Vector3 position, Vector3 color, float intensity, float radius)
        {
            if (pointLights.Count == RenderGraph.MAX_POINT_LIGHTS)
            {
                throw new Exception("Max. number of point lights reached.");
            }

            PointLight light = new PointLight();
            light.internalLight = new GPUPointLight()
            {
                position = new Vector4(position, 1),
                color = new Vector4(color, intensity),
                range = new Vector4(radius, 0, 0.7f, 1.8f)
            };
            if (emptyPointLights.Count > 0)
            {
                pointLights[emptyPointLights.First()] = light;
                UpdatePointLight(emptyPointLights.First());
                emptyPointLights.RemoveAt(0);
            } else
            {
                pointLights.Add(light);
                UpdatePointLight(pointLights.Count - 1);
            }
            return light;
        }

        public static void RemovePointLight(PointLight light)
        {
            int index = pointLights.IndexOf(light);
            PointLight light_ = pointLights[index];

            light_.enabled = false;

            UpdatePointLight(index);
            emptyPointLights.Add(index);

        }

        public static void UpdatePointLight(int index)
        {
            unsafe
            {
                int size = sizeof(GPUPointLight);
                GPUPointLight pointLight = pointLights[index].internalLight;
                packedLightSSBO.SetData(pointLightOffset(index), size, pointLight);
                packedLightSSBO.SetData(0, sizeof(uint), pointLights.Count);
            }
        }




        public static SpotLight AddSpotLight(Vector3 position, Vector3 color, Vector3 direction, float intensity, float radius, float innerCutoff, float outerCutoff)
        {
            if (spotLights.Count == RenderGraph.MAX_SPOT_LIGHTS)
            {
                throw new Exception("Max. number of spot lights reached.");
            }

            SpotLight light = new SpotLight();
            light.internalLight = new GPUSpotLight()
            {
                position = new Vector4(position, 1),
                color = new Vector4(color, intensity),
                direction = new Vector4(direction, radius),
                innerCutoff = innerCutoff,
                outerCutoff = outerCutoff
            };
            if (emptySpotLights.Count > 0)
            {
                spotLights[emptySpotLights.First()] = light;
                UpdateSpotLight(emptySpotLights.First());
                emptySpotLights.RemoveAt(0);
            } else
            {
                spotLights.Add(light);
                UpdateSpotLight(spotLights.Count - 1);
            }
            return light;
        }

        public static void RemoveSpotLight(SpotLight light)
        {
            int index = spotLights.IndexOf(light);
            SpotLight light_ = spotLights[index];

            light_.enabled = false;

            UpdateSpotLight(index);
            emptySpotLights.Add(index);
        }

        public static void UpdateSpotLight(int index)
        {
            unsafe
            {
                int size = sizeof(GPUSpotLight);
                GPUSpotLight spotLight = spotLights[index].internalLight;
                packedLightSSBO.SetData(spotLightOffset(index), size, spotLight);
                packedLightSSBO.SetData(sizeof(uint), sizeof(uint), spotLights.Count);
            }
        }


        public static DirectionalLight AddDirectionalLight(Vector3 direction, Vector3 color, float intensity)
        {
            DirectionalLight light = new DirectionalLight();
            light.internalLight = new GPUDirectionalLight()
            {
                Direction = new Vector4(direction, 1),
                Color = new Vector4(color, intensity),
            };

            directionalLight = light;
            return light;
        }

        public static void RemoveDirectionalLight(DirectionalLight light)
        {
            DirectionalLight light_ = directionalLight;
            light_.enabled = false;

            UpdateDirectionalLight();
        }

        public static void UpdateDirectionalLight()
        {
            unsafe
            {
                int size = sizeof(GPUDirectionalLight);
                GPUDirectionalLight light = directionalLight.internalLight;
                packedLightSSBO.SetData(directionalLightOffset(), size, light);
                packedLightSSBO.SetData(2 * sizeof(uint), sizeof(uint), 1);
            }
        }


    }
}
