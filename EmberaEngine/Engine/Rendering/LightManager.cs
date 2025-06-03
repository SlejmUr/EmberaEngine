using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmberaEngine.Engine.Utilities;
using OpenTK.Graphics.OpenGL;

namespace EmberaEngine.Engine.Rendering
{
    public static class LightManager
    {
        static List<PointLight> pointLights;
        static List<int> emptyPointLights;

        static List<SpotLight> spotLights;
        static List<int> emptySpotLights;

        static DirectionalLight directionalLight;

        static BufferObject<GPUDirectionalLight> directionalLightSSBO;
        static BufferObject<GPUPointLight> pointLightSSBO;
        static BufferObject<GPUSpotLight> spotLightSSBO;

        public static void Initialize()
        {
            pointLights = new List<PointLight>();
            emptyPointLights = new List<int>();

            spotLights = new List<SpotLight>();
            emptySpotLights = new List<int>();

            directionalLight = new DirectionalLight();

            
            pointLightSSBO = new BufferObject<GPUPointLight>(BufferStorageTarget.ShaderStorageBuffer, new GPUPointLight[RenderGraph.MAX_POINT_LIGHTS], BufferUsageHint.DynamicCopy);
            spotLightSSBO = new BufferObject<GPUSpotLight>(BufferStorageTarget.ShaderStorageBuffer, new GPUSpotLight[RenderGraph.MAX_SPOT_LIGHTS], BufferUsageHint.DynamicCopy);
            directionalLightSSBO = new BufferObject<GPUDirectionalLight>(BufferStorageTarget.ShaderStorageBuffer, new GPUDirectionalLight(), BufferUsageHint.DynamicCopy);
        }

        public static BufferObject<GPUPointLight> GetPointLightSSBO()
        {
            return pointLightSSBO;
        }

        public static BufferObject<GPUSpotLight> GetSpotLightSSBO()
        {
            return spotLightSSBO;
        }

        public static BufferObject<GPUDirectionalLight> GetDirectionalLightSSBO()
        {
            return directionalLightSSBO;
        }

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
                range = radius
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
                pointLightSSBO.SetData(index * size, size, pointLight);
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
                spotLightSSBO.SetData(index * size, size, spotLight);
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
                directionalLightSSBO.SetData(0, size, light);
            }
        }

    }
}
