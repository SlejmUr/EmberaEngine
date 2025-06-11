using EmberaEngine.Engine.Utilities;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Utils
{
    public static class Guizmo3D
    {
        static Shader GizmoTextureShader;
        static Shader lineMeshShader;

        static Camera renderCamera;

        static Mesh Cube;
        static Mesh Quad;
        static Mesh Circle;

        public static void Initialize()
        {
            GizmoTextureShader = new Shader("Editor/Assets/Shaders/gizmoTexture");
            lineMeshShader = new Shader("Editor/Assets/Shaders/base");

            Cube = Graphics.GetWireFrameCube();
            Quad = Graphics.GetQuad();
            Circle = Graphics.GetCircle();
        }

        public static void Render()
        {
            renderCamera = Renderer3D.GetRenderCamera();
        }

        public static void RenderCube(Vector3 position, Vector3 scale, Vector3 rotation)
        {
            GraphicsState.SetLineSmooth(true);
            if (renderCamera == null) { return; }
            Matrix4 modelMatrix = Matrix4.CreateScale(scale);
            modelMatrix *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotation.X));
            modelMatrix *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotation.Y));
            modelMatrix *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation.Z));
            modelMatrix *= Matrix4.CreateTranslation(position);

            lineMeshShader.Use();
            lineMeshShader.SetMatrix4("W_MODEL_MATRIX", modelMatrix);
            lineMeshShader.SetMatrix4("W_PROJECTION_MATRIX", renderCamera.GetProjectionMatrix());
            lineMeshShader.SetMatrix4("W_VIEW_MATRIX", renderCamera.GetViewMatrix());

            GraphicsState.SetDepthTest(true);
            GraphicsState.SetDepthMask(false);
            GraphicsState.SetPolygonMode(OpenTK.Graphics.OpenGL.MaterialFace.FrontAndBack, OpenTK.Graphics.OpenGL.PolygonMode.Line);
            GraphicsState.SetLineWidth(2);

            Cube.VAO.Render(OpenTK.Graphics.OpenGL.PrimitiveType.Lines);

            GraphicsState.SetPolygonMode(OpenTK.Graphics.OpenGL.MaterialFace.FrontAndBack, OpenTK.Graphics.OpenGL.PolygonMode.Fill);
            GraphicsState.SetDepthMask(true);
        }

        public static void RenderLightCircle(Vector3 position, Vector3 scale, Vector3 rotation)
        {
            GraphicsState.SetLineSmooth(true);
            if (renderCamera == null) { return; }
            scale *= 2;
            Matrix4 modelMatrix = Matrix4.CreateScale(scale);
            modelMatrix *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotation.X));
            modelMatrix *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotation.Y));
            modelMatrix *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation.Z));
            modelMatrix *= Matrix4.CreateTranslation(position);

            lineMeshShader.Use();
            lineMeshShader.SetMatrix4("W_MODEL_MATRIX", modelMatrix);
            lineMeshShader.SetMatrix4("W_PROJECTION_MATRIX", renderCamera.GetProjectionMatrix());
            lineMeshShader.SetMatrix4("W_VIEW_MATRIX", renderCamera.GetViewMatrix());

            GraphicsState.SetDepthTest(true);
            GraphicsState.SetDepthMask(false);
            GraphicsState.SetPolygonMode(OpenTK.Graphics.OpenGL.MaterialFace.FrontAndBack, OpenTK.Graphics.OpenGL.PolygonMode.Line);
            GraphicsState.SetLineWidth(2);

            Circle.VAO.Render(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);

            modelMatrix = Matrix4.CreateScale(scale);
            modelMatrix *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotation.X));
            modelMatrix *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotation.Y + 90));
            modelMatrix *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation.Z));
            modelMatrix *= Matrix4.CreateTranslation(position);

            lineMeshShader.SetMatrix4("W_MODEL_MATRIX", modelMatrix);

            Circle.VAO.Render(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);

            modelMatrix = Matrix4.CreateScale(scale);
            modelMatrix *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotation.X + 90));
            modelMatrix *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotation.Y));
            modelMatrix *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation.Z));
            modelMatrix *= Matrix4.CreateTranslation(position);

            lineMeshShader.SetMatrix4("W_MODEL_MATRIX", modelMatrix);

            Circle.VAO.Render(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);

            GraphicsState.SetPolygonMode(OpenTK.Graphics.OpenGL.MaterialFace.FrontAndBack, OpenTK.Graphics.OpenGL.PolygonMode.Fill);
            GraphicsState.SetDepthMask(true);
        }

        public static void RenderCircle(Vector3 position, Vector3 scale, Vector3 rotation)
        {
            GraphicsState.SetLineSmooth(true);
            if (renderCamera == null) { return; }

            Matrix4 modelMatrix = Matrix4.CreateScale(scale);
            modelMatrix *= Matrix4.CreateRotationX(rotation.X);
            modelMatrix *= Matrix4.CreateRotationY(rotation.Y);
            modelMatrix *= Matrix4.CreateRotationZ(rotation.Z);
            modelMatrix *= Matrix4.CreateTranslation(position);

            lineMeshShader.Use();
            lineMeshShader.SetMatrix4("W_MODEL_MATRIX", modelMatrix);
            lineMeshShader.SetMatrix4("W_PROJECTION_MATRIX", renderCamera.GetProjectionMatrix());
            lineMeshShader.SetMatrix4("W_VIEW_MATRIX", renderCamera.GetViewMatrix());


            GraphicsState.SetPolygonMode(OpenTK.Graphics.OpenGL.MaterialFace.FrontAndBack, OpenTK.Graphics.OpenGL.PolygonMode.Line);
            GraphicsState.SetLineWidth(2);


            Circle.VAO.Render(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);

            GraphicsState.SetPolygonMode(OpenTK.Graphics.OpenGL.MaterialFace.FrontAndBack, OpenTK.Graphics.OpenGL.PolygonMode.Fill);
        }

        public static void RenderTexture(Texture texture, Vector3 position, Vector3 scale)
        {
            if (renderCamera == null) { return; }

            GizmoTextureShader.Use();
            GizmoTextureShader.SetInt("INPUT_TEXTURE", 0);

            Matrix4 modelMatrix = Matrix4.CreateScale(scale) * Matrix4.LookAt(position, renderCamera.position, -Vector3.UnitY);
            modelMatrix.Invert();

            GizmoTextureShader.SetMatrix4("W_MODEL_MATRIX", modelMatrix);
            GizmoTextureShader.SetMatrix4("W_PROJECTION_MATRIX", renderCamera.GetProjectionMatrix());
            GizmoTextureShader.SetMatrix4("W_VIEW_MATRIX", renderCamera.GetViewMatrix());

            GraphicsState.SetTextureActiveBinding(TextureUnit.Texture0);
            texture.Bind();

            Quad.Draw();

        }
    }
}
