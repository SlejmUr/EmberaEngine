using EmberaEngine.Engine.Core;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Components
{
    public enum ColliderShapeType
    {
        Box,
        Sphere,
        Capsule,
        ConvexHull,
        Mesh
    }

    public class ColliderComponent3D : Component
    {
        public override string Type => nameof(ColliderComponent3D);

        private ColliderShapeType colliderShape;
        private Vector3 size = Vector3.One;
        private float radius = 1.0f;

        private PhysicsObjectHandle physicsObjectHandle;

        public Action OnColliderPropertyChanged = () => {};

        public ColliderShapeType ColliderShape
        {
            get => colliderShape;
            set
            {
                OnColliderPropertyChanged.Invoke();
            }
        }

        public Vector3 Size
        {
            get => size;
            set
            {
                size = value;
                OnColliderPropertyChanged.Invoke();
            }
        }

        public float Radius
        {
            get => radius;
            set
            {
                radius = value;
                OnColliderPropertyChanged.Invoke();
            }
        }


        public override void OnStart()
        {
            
        }

        public override void OnUpdate(float dt)
        {

        }

    }
}
