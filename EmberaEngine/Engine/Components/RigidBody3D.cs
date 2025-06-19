﻿using BepuPhysics;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;

using OpenTK.Mathematics;
using System.Reflection.Metadata;

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

        private Rigidbody3DType type = Rigidbody3DType.Dynamic;
        private float mass = 1f;
        private float friction = 0.5f;
        private float restitution = 0.1f;

        private ColliderComponent3D collider;
        private PhysicsObjectHandle handle;

        public Rigidbody3DType RigidbodyType
        {
            get => type;
            set
            {
                type = value;
                RecreateHandle();
            }
        }

        public float Mass
        {
            get => mass;
            set
            {
                mass = Math.Max(0.0001f, value);
                RecreateHandle();
            }
        }

        public float Friction
        {
            get => friction;
            set
            {
                friction = value;
                RecreateHandle();
            }
        }

        public float Restitution
        {
            get => restitution;
            set
            {
                restitution = value;
                RecreateHandle();
            }
        }

        public override void OnStart()
        {
            collider = gameObject.GetComponent<ColliderComponent3D>();
            if (collider != null)
                collider.OnColliderPropertyChanged += RecreateHandle;

            gameObject.Scene.OnComponentAdded += OnComponentAddedCallback;
            gameObject.Scene.OnComponentRemoved += OnComponentRemovedCallback;

            RecreateHandle();
        }

        public override void OnUpdate(float dt)
        {
            if (handle.IsStatic && gameObject.transform.hasMoved)
            {
                RecreateHandle();
                return;
            }

            var body = handle.BodyReference;
            if (body.Exists)
            {
                var pose = body.Pose;
                gameObject.transform.Position = Helper.ToVector3(pose.Position);
                gameObject.transform.Rotation = Helper.ToDegrees(Helper.ToEulerAngles(pose.Orientation));
            }
        }

        public void ApplyForce(Vector3 force)
        {
            this.gameObject.Scene.PhysicsManager3D.ApplyForce(handle.BodyHandle, new System.Numerics.Vector3(force.X, force.Y, force.Z));
        }

        public void ApplyImpulse(Vector3 impulse)
        {
            this.gameObject.Scene.PhysicsManager3D.ApplyImpulse(handle.BodyHandle, new System.Numerics.Vector3(impulse.X, impulse.Y, impulse.Z));
        }

        public void ApplyTorque(Vector3 torque)
        {
            this.gameObject.Scene.PhysicsManager3D.ApplyTorque(handle.BodyHandle, new System.Numerics.Vector3(torque.X, torque.Y, torque.Z));
        }

        public void SetVelocity(Vector3 velocity)
        {
            this.gameObject.Scene.PhysicsManager3D.SetVelocity(handle.BodyHandle, new System.Numerics.Vector3(velocity.X, velocity.Y, velocity.Z));
        }

        public Vector3 GetVelocity()
        {
            return Helper.ToVector3(this.gameObject.Scene.PhysicsManager3D.GetVelocity(handle.BodyHandle));
        }

        private void RecreateHandle()
        {
            var pm = gameObject.Scene?.PhysicsManager3D;
            if (pm == null || collider == null)
                return;

            if ((handle.IsStatic && pm.StaticExists(handle.StaticHandle))
                || (!handle.IsStatic && pm.DynamicExists(handle.BodyHandle)))
            {
                pm.RemovePhysicsObject(handle);
            }

            handle = pm.AddPhysicsObject(gameObject.transform, this, collider);
        }

        private void OnComponentAddedCallback(Component comp)
        {
            if (comp.gameObject != gameObject)
                return;
            if (comp is ColliderComponent3D newCol)
            {
                collider = newCol;
                collider.OnColliderPropertyChanged += RecreateHandle;
                RecreateHandle();
            }
        }

        private void OnComponentRemovedCallback(Component comp)
        {
            if (comp.gameObject != gameObject)
                return;
            if (comp is ColliderComponent3D oldCol)
            {
                oldCol.OnColliderPropertyChanged -= RecreateHandle;
                collider = null;
                gameObject.Scene?.PhysicsManager3D.RemovePhysicsObject(handle);
            }
        }
    }
}
