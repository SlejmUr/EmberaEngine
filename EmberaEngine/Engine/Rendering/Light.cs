using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{
    public enum LightType
    {
        PointLight,
        SpotLight,
        DirectionalLight
    }

    public enum LightAttenuationType
    {
        Custom = 0,
        Constant = 1,
        Linear = 2,
        Quadratic = 3,
    }

    public class DirectionalLight
    {
        public GPUDirectionalLight internalLight;
        public bool needsUpdate;

        public Vector3 direction
        {
            get => this.internalLight.Direction.Xyz;
            set
            {
                this.internalLight.Direction.Xyz = value;
                this.needsUpdate = true;
            }
        }

        public bool enabled
        {
            get => this.internalLight.Direction.W == 1;
            set
            {
                this.internalLight.Direction.W = value ? 1 : 0;
                this.needsUpdate = true;
            }
        }

        public Vector3 color
        {
            get => this.internalLight.Color.Xyz;
            set
            {
                this.internalLight.Color.Xyz = value;
                this.needsUpdate = true;
            }
        }

        public float intensity
        {
            get => this.internalLight.Color.W;
            set
            {
                this.internalLight.Color.W = value;
                this.needsUpdate = true;
            }
        }
    }

    public class PointLight
    {
        public GPUPointLight internalLight;
        public bool needsUpdate;

        public Vector3 position
        {
            get => this.internalLight.position.Xyz;
            set
            {
                this.internalLight.position.Xyz = value;
                this.needsUpdate = true;
            }
        }

        public Vector3 Color
        {
            get => this.internalLight.color.Xyz;
            set
            {
                this.internalLight.color.Xyz = value;
                this.needsUpdate = true;
            }
        }

        public bool enabled
        {
            get => this.internalLight.position.W == 1;
            set
            {
                this.internalLight.position.W = value ? 1 : 0;
                this.needsUpdate = true;
            }
        }

        public float intensity
        {
            get => this.internalLight.color.W;
            set
            {
                this.internalLight.color.W = value;
                this.needsUpdate = true;
            }
        }

        public float range
        {
            get => this.internalLight.range.X;
            set
            {
                this.internalLight.range.X = value;
                this.needsUpdate = true;
            }
        }

        public int attenuationType
        {
            get => (int)this.internalLight.range.Y;
            set {
                this.internalLight.range.Y = value;
                this.needsUpdate = true;
            }
        }

        public Vector2 attenuationParameters
        {
            get => this.internalLight.range.Zw;
            set
            {
                this.internalLight.range.Zw = value;
                this.needsUpdate = true;
            }
        }
    }

    public class SpotLight
    {
        public GPUSpotLight internalLight;
        public bool needsUpdate;

        public Vector3 position
        {
            get => this.internalLight.position.Xyz;
            set
            {
                this.internalLight.position.Xyz = value;
                this.needsUpdate = true;
            }
        }

        public Vector3 Color
        {
            get => this.internalLight.color.Xyz;
            set
            {
                this.internalLight.color.Xyz = value;
                this.needsUpdate = true;
            }
        }

        public bool enabled
        {
            get => this.internalLight.position.W == 1;
            set
            {
                this.internalLight.position.W = value ? 1 : 0;
                this.needsUpdate = true;
            }
        }

        public float intensity
        {
            get => this.internalLight.color.W;
            set
            {
                this.internalLight.color.W = value;
                this.needsUpdate = true;
            }
        }

        public float range
        {
            get => this.internalLight.direction.W;
            set
            {
                this.internalLight.direction.W = value;
                this.needsUpdate = true;
            }
        }

        public Vector3 direction
        {
            get => this.internalLight.direction.Xyz;
            set
            {
                this.internalLight.direction.Xyz = value;
                this.needsUpdate = true;
            }
        }

        public float innerCutoff
        {
            get => this.internalLight.innerCutoff;
            set
            {
                this.internalLight.innerCutoff = value;
                this.needsUpdate = true;
            }
        }

        public float outerCutoff
        {
            get => this.internalLight.outerCutoff;
            set
            {
                this.internalLight.outerCutoff = value;
                this.needsUpdate = true;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct GPULightData
    {
        public uint numPointLights;
        public uint numSpotLights;
        public uint numDirectionalLights;
        public uint _padding; // Ensure 16-byte alignment

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)]
        public GPUPointLight[] pointLights;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public GPUSpotLight[] spotLights;

        public GPUDirectionalLight directionalLights;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public unsafe struct GPUDirectionalLight
    {
        public Vector4 Direction; // Normalized + w for enabled/disabled
        public Vector4 Color; // Color + w for intensity
    }


    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public unsafe struct GPUPointLight
    {
        public Vector4 position; // 3 floats for position 1 float enabled/disabled
        public Vector4 color; // 3 floats for color 1 float for intensity
        public Vector4 range; // 1 float range, 1 float attenuation type, 2 floats attenuation parameters (Linear Factor and QuadraticFactor)

        //private fixed byte _padding[12];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public unsafe struct GPUSpotLight
    {
        public Vector4 position; // xyz position w enabled
        public Vector4 color; // xyz color w intensity
        public Vector4 direction; // xyz direction w range
        public float innerCutoff;
        public float outerCutoff;

        private fixed byte _padding[8];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public unsafe struct LightData
    {
        public uint pointLightCount;
        public uint directionalLightCount;
        public uint spotLightCount;
        private fixed byte _padding[4];
    }
}
