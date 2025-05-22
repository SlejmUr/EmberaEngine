using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{





    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public unsafe struct PointLight
    {
        public Vector4 position;
        public Vector4 color;
        public bool enabled;
        public float intensity;
        public float range;

        private fixed byte _padding[4];
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
