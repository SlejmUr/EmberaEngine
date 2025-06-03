using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Components
{
    public class Transform : Component
    {
        public override string Type => nameof(Transform);

        public Vector3 position = Vector3.Zero;
        public Vector3 rotation = Vector3.Zero;
        public Vector3 scale = Vector3.One;

        public bool hasMoved = false;

        private Vector3 prev_position;
        private Vector3 prev_rotation;

        
        public override void OnStart()
        {
            prev_position = position;
            prev_rotation = rotation;
        }

        public override void OnUpdate(float dt)
        {
            if (prev_position != position || prev_rotation != rotation)
            {
                prev_position = position;
                prev_rotation = rotation;

                hasMoved = true;
            } else
            {
                hasMoved = false;
            }
        }



    }
}
