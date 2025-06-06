using EmberaEngine.Engine.Core;
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

        private Vector3 position = Vector3.Zero;
        private Vector3 rotation = Vector3.Zero;
        private Vector3 scale = Vector3.One;
        
        private Matrix4 localMatrix;
        private Matrix4 worldMatrix;

        private Vector3 prev_position;
        private Vector3 prev_rotation;
        private Vector3 prev_scale;

        public bool hasMoved = false;

        public Vector3 Position
        {
            get { return position; }
            set
            {
                position = value;
                hasMoved = true;
                UpdateTransform();
            }
        }

        public Vector3 Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                hasMoved = true;
                UpdateTransform();
            }
        }

        public Vector3 Scale
        {
            get { return scale; }
            set
            {

                Console.WriteLine(scale);
                scale = value;
                hasMoved = true;
                UpdateTransform();
            }
        }


        public Matrix4 GetWorldMatrix() => worldMatrix;

        public void UpdateTransform()
        {
            // Construct local matrix from position, rotation, and scale
            localMatrix =
                Matrix4.CreateScale(scale) *
                Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotation.X)) *
                Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotation.Y)) *
                Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation.Z)) *
                Matrix4.CreateTranslation(position);

            // Combine with parent's world matrix, if any
            if (gameObject.parentObject != null)
            {
                var parentTransform = gameObject.parentObject.transform;
                if (parentTransform != null)
                {
                    worldMatrix = localMatrix * parentTransform.worldMatrix;
                }
                else
                {
                    worldMatrix = localMatrix;
                }
            }
            else
            {
                worldMatrix = localMatrix;
            }


            // Recursively update children
            foreach (var child in gameObject.children)
            {
                var childTransform = child.GetComponent<Transform>();
                childTransform?.UpdateTransform();
            }
        }


        public override void OnStart()
        {
            UpdateTransform();
            prev_position = position;
            prev_rotation = rotation;
            prev_scale = scale;
        }

        public override void OnUpdate(float dt)
        {
            if (prev_position != position || prev_rotation != rotation || prev_scale != scale)
            {
                prev_position = position;
                prev_rotation = rotation;
                prev_scale = scale;

                hasMoved = true;
            }
            else
            {
                hasMoved = false;
            }
        }



    }
}
