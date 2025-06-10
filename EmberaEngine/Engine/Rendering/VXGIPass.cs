using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{
    class VXGIPass : IRenderPass
    {
        bool isActive = true;


        Vector3i VolumeTextureDimensions = new Vector3i(150, 80, 64);


        public void Initialize(int width, int height)
        {

        }


        public void Apply(FrameData frameData)
        {

        }

        public Framebuffer GetOutputFramebuffer()
        {
            throw new NotImplementedException();
        }

        public void Resize(int width, int height)
        {

        }

        public bool GetState() => isActive;
        public void SetState(bool value) => isActive = value;
    }
}
