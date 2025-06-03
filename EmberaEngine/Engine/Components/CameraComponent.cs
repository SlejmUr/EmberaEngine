using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Components
{
    public class CameraComponent3D : Component
    {
        public override string Type => nameof(CameraComponent3D);

        public Color4 ClearColor;

        private float _fovy = MathHelper.DegreesToRadians(45.0f);
        private float nearClip = .1f, farClip = 1000f;

        private int width = Screen.Size.X, height = Screen.Size.Y;
        private int prevWidth, prevHeight;
        private float aspectRatio;

        private Vector3 _front = Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fovy);
            set
            {
                float fovV = MathHelper.DegreesToRadians(value);
                if (fovV <= 0f || fovV > Math.PI) return;
                _fovy = fovV;
                SetCameraProperties();
            }
        }

        public bool isDefault
        {
            get => camera.isDefault;
            set
            {
                camera.isDefault = value;
                if (value)
                {
                    this.gameObject.scene.SetMainCamera(this);
                }
            }
        }


        internal Camera camera;

        public CameraComponent3D()
        {
            camera = new Camera();

            SetCameraProperties();
        }

        public override void OnStart()
        {
            this.gameObject.scene.AddCamera(this);
        }

        public override void OnUpdate(float dt)
        {
            camera.position = gameObject.transform.position;
            camera.SetClearColor(ClearColor);
            camera.nearClip = nearClip;
            camera.farClip = farClip;
            camera.fovy = _fovy;
            UpdateCameraVectors();

            width = Screen.Size.X;
            height = Screen.Size.Y;

            if (prevWidth != width || prevHeight != height)
            {
                prevWidth = width; prevHeight = height;
                SetCameraProperties();
                Console.WriteLine("Resized");
            }
        }

        public override void OnDestroy()
        {
            this.gameObject.scene.RemoveCamera( this );
        }

        private void SetCameraProperties()
        {
            aspectRatio = (float)width / height;

            camera.SetProjectionMatrix(
                Matrix4.CreatePerspectiveFieldOfView(_fovy, aspectRatio, nearClip, farClip)
            );
        }

        private void UpdateCameraVectors()
        {
            float PITCH = MathHelper.DegreesToRadians(gameObject.transform.rotation.Y);
            float YAW = MathHelper.DegreesToRadians(gameObject.transform.rotation.X);

            // First, the front matrix is calculated using some basic trigonometry.
            _front.X = MathF.Cos(PITCH) * MathF.Cos(YAW);
            _front.Y = MathF.Sin(PITCH);
            _front.Z = MathF.Cos(PITCH) * MathF.Sin(YAW);

            // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
            _front = Vector3.Normalize(_front);

            // Calculate both the right and the up vector using cross product.
            // Note that we are calculating the right from the global up; this behaviour might
            // not be what you need for all cameras so keep this in mind if you do not want a FPS camera.
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
            camera.SetViewMatrix(Matrix4.LookAt(gameObject.transform.position, gameObject.transform.position + _front, _up));
        }

    }
}
