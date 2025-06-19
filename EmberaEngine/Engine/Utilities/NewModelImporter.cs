using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using OpenTK.Mathematics;
using Assimp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Utilities
{
    public class NewModelImporter
    {
        public struct ModelNode
        {
            public string name;
            public Vector3 position;
            public Vector3 rotation;
            public List<ModelNode> children;
        }

        public struct ModelLightData {
            public LightType lightType;
            public Color4 colorDiffuse;
            public float intensity;
            public float radius;
        }

        public struct ModelCameraData
        {
            
        }

        public struct ModelMeshData
        {
            public Mesh mesh;
        }

        public static ModelNode Load(string resourcePath)
        {
            string diskPath = VirtualFileSystem.ResolvePath(resourcePath);

            if (!VirtualFileSystem.Exists(resourcePath))
            {
                Console.WriteLine("[MODEL IMPORTER]: The model file does not exist at the specified location!");
                return default;
            }

            AssimpContext importer = new AssimpContext();
            Assimp.Scene scene;

            try
            {
                scene = importer.ImportFile(diskPath,
                    PostProcessSteps.Triangulate |
                    PostProcessSteps.GenerateNormals |
                    PostProcessSteps.CalculateTangentSpace |
                    PostProcessSteps.FlipUVs |
                    PostProcessSteps.GenerateUVCoords |
                    PostProcessSteps.OptimizeMeshes
                );
            }
            catch ( Exception ex )
            {
                Console.WriteLine($"[Model Importer]: Failed to load model at \"{resourcePath}\"\nError: {ex}");
                return default;
            }


            if (scene?.RootNode == null) 
                return default;





            return default;
        }

        static void ProcessNode(Assimp.Node node, Assimp.Scene scene)
        {
            node.Transform.Decompose(out Assimp.Vector3D scaling, out Assimp.Quaternion rotation, out Assimp.Vector3D position);

            Vector3D Rotation = ToEulerAngles(rotation);
            Vector3 openTKRotation = new Vector3(Rotation.X, Rotation.Y, Rotation.Z);

            ModelNode newNode = new ModelNode()
            {
                name = node.Name,
                position = new Vector3(position.X, position.Y, position.Z),
                rotation = openTKRotation,
                children = new List<ModelNode>()
            };




        }








        public static Vector3D ToEulerAngles(Assimp.Quaternion q)
        {
            // Convert quaternion to rotation matrix
            Matrix3x3 mat = q.GetMatrix();

            float sy = MathF.Sqrt(mat.A1 * mat.A1 + mat.B1 * mat.B1);

            bool singular = sy < 1e-6f;

            float x, y, z; // Euler angles

            if (!singular)
            {
                x = MathF.Atan2(mat.C2, mat.C3); // Pitch
                y = MathF.Atan2(-mat.C1, sy);    // Yaw
                z = MathF.Atan2(mat.B1, mat.A1); // Roll
            }
            else
            {
                x = MathF.Atan2(-mat.B3, mat.B2);
                y = MathF.Atan2(-mat.C1, sy);
                z = 0;
            }

            return new Vector3D(
                MathHelper.RadiansToDegrees(x),
                MathHelper.RadiansToDegrees(y),
                MathHelper.RadiansToDegrees(z)
            );
        }
    }
}
