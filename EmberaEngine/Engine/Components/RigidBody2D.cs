using EmberaEngine.Engine.Core;
using nkast.Aether.Physics2D.Dynamics;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Components
{
    public class RigidBody2D : Component
    {
        public override string Type => nameof(RigidBody2D);

        public EmberaEngine.Engine.Core.BodyType BodyType { get; set; } = Core.BodyType.Static;
        public bool freezeRotation = false;

        private Body body;
        private nkast.Aether.Physics2D.Common.Vector2 bodyTransform = new nkast.Aether.Physics2D.Common.Vector2();

        public override void OnStart()
        {
            body = gameObject.Scene.PhysicsManager2D.CreateBox(BodyType, new Vector2(gameObject.transform.Position.X / PhysicsManager2D.PPM, gameObject.transform.Position.Y / PhysicsManager2D.PPM), MathHelper.DegreesToRadians(gameObject.transform.Rotation.X), (gameObject.transform.Scale.X / PhysicsManager2D.PPM * 2), (gameObject.transform.Scale.Y / PhysicsManager2D.PPM * 2), 1f, Vector2.Zero);

            //gameObject.transform.Scale = new Vector3((gameObject.transform.Scale.X / PhysicsManager2D.PPM) / 2, (gameObject.transform.Scale.Y / PhysicsManager2D.PPM) / 2, 1);
        }

        public void ApplyImpulse(Vector2 impulse)
        {
            body.ApplyLinearImpulse(new nkast.Aether.Physics2D.Common.Vector2(impulse.X, impulse.Y));
        }

        public void ApplyForce(Vector2 force)
        {
            nkast.Aether.Physics2D.Common.Vector2 forceRef = new nkast.Aether.Physics2D.Common.Vector2(force.X, force.Y);
            body.ApplyForce(ref forceRef);
        }

        public void SetRestitution(float value)
        {
            body.SetRestitution(value);
        }

        public void SetFriction(float value)
        {
            body.SetFriction(value);
        }

        public void SetMass(float mass)
        {
            body.Mass = mass;
        }

        public void Rotate(float angle)
        {
            gameObject.transform.Rotation = new Vector3(angle, gameObject.transform.Rotation.Y, gameObject.transform.Rotation.Z);
            body.Rotation = MathHelper.DegreesToRadians(angle);
        }

        public override void OnUpdate(float dt)
        {

            if (BodyType == Core.BodyType.Static)
            {
                return;
            }


            gameObject.transform.Position = new (body.Position.X * PhysicsManager2D.PPM, (body.Position.Y * PhysicsManager2D.PPM), gameObject.transform.Position.Z);// + gameObject.transform.Scale.X * 2;

            if (!freezeRotation)
            {
                gameObject.transform.Rotation = new Vector3(MathHelper.RadiansToDegrees(body.Rotation), gameObject.transform.Rotation.Y, gameObject.transform.Rotation.Z);
            }

        }



    }
}