using BepuPhysics;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;

namespace EmberaEngine.Engine.Components
{
    public enum Rigidbody3DType
    {
        Static,
        Dynamic,
        Kinematic
    }

    public class RigidBody3D : Component
    {
        public override string Type => nameof(RigidBody3D);

        private Rigidbody3DType rigidBodyType;
        private float velocity;
        private float mass = 1f;
        private Vector3 gravity = Helper.ToVector3(PhysicsManager3D.GlobalGravity);
        private ColliderComponent3D collider;
        private PhysicsObjectHandle physicsObjectHandle;

        public Rigidbody3DType Rigidbody3DType
        {
            get => rigidBodyType;
            set
            {
                rigidBodyType = value;
                OnValueChanged();
            }
        }

        public float Velocity
        {
            get => velocity;
            set => velocity = value;
        }

        public float Mass
        {
            get => mass;
            set
            {
                mass = value;
                OnValueChanged();
            }
        }

        public Vector3 Gravity
        {
            get => gravity;
            set => gravity = value;
        }

        public override void OnStart()
        {
            collider = gameObject.GetComponent<ColliderComponent3D>();

            gameObject.scene.OnComponentAdded += OnComponentAddedCallback;
            gameObject.scene.OnComponentRemoved += OnComponentRemovedCallback;

            if (collider != null)
            {
                collider.OnColliderPropertyChanged += OnValueChanged;
                physicsObjectHandle = gameObject.scene.PhysicsManager3D
                    .AddPhysicsObject(gameObject.transform, this, collider);
            }
        }

        public override void OnUpdate(float dt)
        {
            if (collider == null) return;
            if (physicsObjectHandle.IsStatic && gameObject.transform.hasMoved)
            {
                OnValueChanged();
            } else if (!physicsObjectHandle.IsStatic)
            {
                var pose = physicsObjectHandle.BodyReference.Pose;
                gameObject.transform.position = Helper.ToVector3(pose.Position);
                gameObject.transform.rotation = Helper.ToDegrees(Helper.ToEulerAngles(pose.Orientation));
            }
        }

        private void OnValueChanged()
        {
            if (collider == null) return;

            var physicsManager = gameObject.scene.PhysicsManager3D;

            if (!physicsObjectHandle.IsStatic &&
                physicsManager.DynamicExists(physicsObjectHandle.BodyHandle))
            {
                physicsManager.RemovePhysicsObject(physicsObjectHandle);
            }
            else if (physicsObjectHandle.IsStatic &&
                     physicsManager.StaticExists(physicsObjectHandle.StaticHandle))
            {
                physicsManager.RemovePhysicsObject(physicsObjectHandle);
            }

            physicsObjectHandle = physicsManager.AddPhysicsObject(gameObject.transform, this, collider);
        }

        private void OnComponentAddedCallback(Component component)
        {
            if (component.gameObject != gameObject ||
                component.Type != nameof(ColliderComponent3D))
                return;

            collider = (ColliderComponent3D)component;
            collider.OnColliderPropertyChanged += OnValueChanged;

            physicsObjectHandle = gameObject.scene.PhysicsManager3D
                .AddPhysicsObject(gameObject.transform, this, collider);
        }

        private void OnComponentRemovedCallback(Component component)
        {
            if (component.gameObject != gameObject ||
                component.Type != nameof(ColliderComponent3D))
                return;

            collider = null;
        }
    }
}
