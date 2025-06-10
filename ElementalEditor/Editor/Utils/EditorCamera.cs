using System;
using OpenTK.Mathematics;
using EmberaEngine.Engine.Utilities;
using EmberaEngine.Engine.Core;

namespace ElementalEditor.Editor.Utils
{
    public class EditorCamera
    {
        private Vector2 _initialMousePos;
        private float _distance = 10.0f;
        private Vector3 _focalPoint = Vector3.Zero;
        private float _pitch = MathHelper.DegreesToRadians(0.0f);
        private float _yaw = MathHelper.DegreesToRadians(180.0f);
        private float _fovy = MathHelper.DegreesToRadians(45.0f);
        private int width = Screen.Size.X, height = Screen.Size.Y;
        private int prevWidth, prevHeight;
        private float _farClip, _nearClip;
        private bool _useArcball;

        public Camera Camera { get; private set; } = new Camera();
        public float Speed { get; set; } = 2.0f;
        public bool IsSelected { get; set; } = false;
        public int ProjectionType { get; set; } = 0;

        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fovy);
            set
            {
                _fovy = MathHelper.DegreesToRadians(value);
                UpdateProjection();
            }
        }

        public EditorCamera(float fov, int width, int height, float farClip, float nearClip)
        {
            _farClip = farClip;
            _nearClip = nearClip;
            SetViewportSize(width, height);
            Fov = fov;
            UpdateView();

            Camera.position = _focalPoint - GetFrontVector() * _distance;
        }

        public void SetPitch(float degrees) => _pitch = MathHelper.DegreesToRadians(degrees);
        public void SetYaw(float degrees) => _yaw = MathHelper.DegreesToRadians(degrees);
        public void SetFov(float degrees) => Fov = degrees;

        public void Update(float deltaTime)
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 delta = (mousePos - _initialMousePos) * 0.005f;
            _initialMousePos = mousePos;

            if (Input.IsPressed(MouseButton.Middle) && Input.GetKey(Keys.LeftShift))
                MousePan(delta);
            else if (Input.IsPressed(MouseButton.Middle))
            {
                ArcballRotate(delta);
                _useArcball = true;
            }

            if (Input.mouseScrollDelta != Vector2.Zero)
                MouseZoom(Input.mouseScrollDelta.Y);

            if (Input.IsPressed(MouseButton.Button2))
            {
                LocalRotate(delta);
                _useArcball = false;
            }

            UpdateView();

            width = Screen.Size.X;
            height = Screen.Size.Y;

            if (prevWidth != width || prevHeight != height)
            {
                prevWidth = width; prevHeight = height;
                UpdateProjection();
                Console.WriteLine("Resized");
            }
        }

        private void ArcballRotate(Vector2 delta)
        {
            float yawSign = GetUpVector().Y < 0 ? -1.0f : 1.0f;
            _yaw += yawSign * delta.X * RotationSpeed();
            _pitch += delta.Y * RotationSpeed();
            _pitch = MathHelper.Clamp(_pitch, MathHelper.DegreesToRadians(-89.0f), MathHelper.DegreesToRadians(89.0f));

            Camera.position = _focalPoint - GetFrontVector() * _distance;
        }

        public void SetViewportSize(int width, int height)
        {
            width = width;
            height = height;
            UpdateProjection();
        }

        public void UpdateProjection()
        {
            Camera.SetProjectionMatrix(
                Matrix4.CreatePerspectiveFieldOfView(_fovy, (float)width / height, _nearClip, _farClip)
            );
        }

        public void UpdateView()
        {
            if (_useArcball)
            {
                Camera.position = _focalPoint - GetFrontVector() * _distance;
                Camera.SetViewMatrix(Matrix4.LookAt(Camera.position, _focalPoint, GetUpVector()));
            }
            else
            {
                Camera.SetViewMatrix(Matrix4.LookAt(Camera.position, Camera.position + GetFrontVector(), GetUpVector()));
            }
        }

        public Vector3 GetPosition() => _focalPoint - GetFrontVector() * _distance;

        public Vector3 GetFrontVector()
        {
            Vector3 front = new(
                MathF.Cos(_pitch) * MathF.Cos(_yaw),
                MathF.Sin(_pitch),
                MathF.Cos(_pitch) * MathF.Sin(_yaw)
            );
            return Vector3.Normalize(front);
        }

        public Vector3 GetRightVector() => Vector3.Normalize(Vector3.Cross(GetFrontVector(), Vector3.UnitY));
        public Vector3 GetUpVector() => Vector3.Normalize(Vector3.Cross(GetRightVector(), GetFrontVector()));
        public Quaternion GetOrientation() => new(new Vector3(-_pitch, _yaw, 0.0f));

        private float[] PanSpeed()
        {
            float factor = MathF.Min(width / 1000f, 2.4f);
            float speed = 0.0366f * (factor * factor) - 0.1778f * factor + 0.3021f;
            return new[] { speed, speed };
        }

        private void MousePan(Vector2 delta)
        {
            float[] panSpeed = PanSpeed();
            Vector3 right = GetRightVector();
            Vector3 up = GetUpVector();

            _focalPoint += -right * delta.X * panSpeed[0] * _distance;
            _focalPoint -= up * delta.Y * panSpeed[1] * _distance;

            Camera.position = _focalPoint - GetFrontVector() * _distance;
        }

        private void LocalRotate(Vector2 delta)
        {
            const float sensitivity = 0.4f;
            _yaw += delta.X * sensitivity;
            _pitch += delta.Y * sensitivity;
            _pitch = MathHelper.Clamp(_pitch, MathHelper.DegreesToRadians(-89.0f), MathHelper.DegreesToRadians(89.0f));

            _focalPoint = Camera.position + GetFrontVector() * _distance;
        }

        private void MouseZoom(float delta)
        {
            _distance -= delta * 0.5f;
            _distance = MathF.Max(_distance, 0.1f);
            Camera.position = _focalPoint - GetFrontVector() * _distance;
        }

        private float RotationSpeed() => 0.8f;

        private float ZoomSpeed()
        {
            float distance = MathF.Min(_distance * 0.2f, 0.0f);
            float speed = MathF.Min(distance * distance, 100.0f);
            return speed;
        }
    }
}
