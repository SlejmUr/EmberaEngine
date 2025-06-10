using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Encodings;
using Assimp;
using OpenTK.Mathematics;
using System.Linq;
using SharpFont;
using System.Text;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Utilities;
using System.Numerics;
using System.Resources;

namespace EmberaEngine.Engine.Utilities
{
    public class ModelImporter
    {
        public struct ModelData
        {
            public GameObject rootObject; // This contains everything.
            public List<GameObject> meshObjects;
            public List<GameObject> cameras;
            public List<GameObject> lights;
        }

        static Dictionary<string, Core.Texture> Textures = new Dictionary<string, Core.Texture>();
        static Dictionary<int, uint> processedMaterialIndices = new Dictionary<int, uint>();

        public static ModelData LoadModel(string path)
        {
            string virtualPath = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            string physicalPath = Path.GetDirectoryName(VirtualFileSystem.ResolvePath(virtualPath));

            var importer = new AssimpContext();
            Assimp.Scene scene;

            try
            {
                scene = importer.ImportFile(VirtualFileSystem.ResolvePath(path),
                    PostProcessSteps.Triangulate |
                    PostProcessSteps.GenerateNormals |
                    PostProcessSteps.CalculateTangentSpace |
                    PostProcessSteps.FlipUVs |
                    PostProcessSteps.GenerateUVCoords |
                    PostProcessSteps.OptimizeGraph |
                    PostProcessSteps.OptimizeMeshes);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ModelImporter] Failed to load model: {virtualPath}\nError: {e.Message}");
                return default;
            }

            if (scene?.RootNode == null) return default;

            List<GameObject> meshObjects = new List<GameObject>();
            List<GameObject> cameras = new List<GameObject>();
            List<GameObject> lights = new List<GameObject>();

            GameObject rootGO = new GameObject();
            rootGO.Name = Path.GetFileNameWithoutExtension(virtualPath);
            int totalMeshCount = scene.MeshCount;
            int meshProcessed = 0;

            Console.WriteLine("VIRTUAL FILE: " + virtualPath);

            // 1. Process materials first
            for (int i = 0; i < scene.MaterialCount; i++)
            {
                if (!processedMaterialIndices.ContainsKey(i))
                {
                    var material = SetupMaterial(i, scene, virtualPath);
                    var materialID = MaterialManager.AddMaterial(material);
                    processedMaterialIndices[i] = materialID;
                }
            }

            // 2. Now process meshes (via node traversal)
            void ProcessNode(Node node, GameObject parentGO)
            {
                GameObject currentGO = new GameObject();
                currentGO.Name = node.Name;
                currentGO.transform.Position = ToOpenTKMatrix(node.Transform).ExtractTranslation();
                parentGO.AddChild(currentGO);

                foreach (int meshIdx in node.MeshIndices)
                {
                    var assimpMesh = scene.Meshes[meshIdx];
                    var processedMesh = ProcessMesh(assimpMesh, scene, node.Transform, virtualPath);

                    var meshGO = new GameObject();
                    meshGO.Name = assimpMesh.Name != "" ? assimpMesh.Name.Substring(0, Math.Min(10, assimpMesh.Name.Length)) : $"Mesh_{meshIdx}";
                    var meshRenderer = meshGO.AddComponent<MeshRenderer>();
                    meshRenderer.SetMesh(processedMesh);

                    currentGO.AddChild(meshGO);
                    meshObjects.Add(meshGO);

                    meshProcessed++;
                    //Console.WriteLine($"[ModelImporter] Progress: {(meshProcessed / (float)totalMeshCount):P0}");
                }

                foreach (var child in node.Children)
                {
                    ProcessNode(child, currentGO);
                }
            }

            ProcessNode(scene.RootNode, rootGO);

            // Process cameras
            foreach (var cam in scene.Cameras)
            {
                GameObject gameObject = new GameObject();
                gameObject.Name = cam.Name;
                var cameraComponent = gameObject.AddComponent<CameraComponent3D>();

                cameraComponent.FarPlane = cam.ClipPlaneFar;
                cameraComponent.NearPlane = cam.ClipPlaneNear;
                cameraComponent.Fov = MathHelper.RadiansToDegrees(cam.FieldOfview);
                gameObject.transform.Position = new OpenTK.Mathematics.Vector3(cam.Position.X, cam.Position.Y, cam.Position.Z);
                gameObject.transform.Rotation = new OpenTK.Mathematics.Vector3(cam.Direction.X, cam.Direction.Y, cam.Direction.Z);

                if (scene.RootNode.FindNode(cam.Name) is Node node)
                {
                    var parent = FindGOByName(rootGO, cam.Name);
                    parent?.AddChild(gameObject);
                }
                else
                {
                    rootGO.AddChild(gameObject);
                }

                cameras.Add(gameObject);
            }


            // Process lights
            foreach (var light in scene.Lights)
            {
                Console.WriteLine("LIGHTS LIGHTS LIGHTS LIGHTS!");
                LightType lightType;

                switch (light.LightType)
                {
                    case LightSourceType.Point:
                        lightType = LightType.PointLight;
                        break;
                    case LightSourceType.Spot:
                        lightType = LightType.SpotLight;
                        break;
                    case LightSourceType.Directional:
                        lightType = LightType.DirectionalLight;
                        break;
                    default:
                        continue;
                }

                GameObject gameObject = new GameObject();
                gameObject.Name = light.Name;
                var lightComponent = gameObject.AddComponent<LightComponent>();

                lightComponent.LightType = lightType;
                lightComponent.Enabled = true;
                lightComponent.Radius = ComputeLightRange(light);
                float intensity = 0.2126f * light.ColorDiffuse.R + 0.7152f * light.ColorDiffuse.G + 0.0722f * light.ColorDiffuse.B;
                lightComponent.Color = new Color4(light.ColorDiffuse.R, light.ColorDiffuse.G, light.ColorDiffuse.B, intensity);
                lightComponent.OuterCutoff = MathHelper.RadiansToDegrees(light.AngleOuterCone) / 2;
                lightComponent.InnerCutoff = MathHelper.RadiansToDegrees(light.AngleInnerCone) / 2;

                // Attach to node if exists
                if (scene.RootNode.FindNode(light.Name) is Node node)
                {
                    // --- Fix the direction: transform it by the node's world matrix ---
                    var direction = new OpenTK.Mathematics.Vector4(light.Direction.X, light.Direction.Y, light.Direction.Z, 0.0f);
                    var position = new OpenTK.Mathematics.Vector4(light.Position.X, light.Position.Y, light.Position.Z, 0.0f);
                    var worldTransform = ToOpenTKMatrix(GetNodeWorldTransform(node)); // You'll need to implement this
                    var transformedDirection = worldTransform * direction; // w = 0 for direction
                    var transformedPosition = worldTransform * position;

                    // Normalize and assign
                    var normalizedRotation = new OpenTK.Mathematics.Vector3((float)transformedDirection.X, (float)transformedDirection.Y, (float)transformedDirection.Z);
                    

                    gameObject.transform.Rotation = normalizedRotation;
                    gameObject.transform.Position = transformedPosition.Xyz;


                    // find GO with same name
                    var parent = FindGOByName(rootGO, light.Name);
                    parent?.AddChild(gameObject);
                }
                else
                {
                    rootGO.AddChild(gameObject);
                }


                lights.Add(gameObject);
            }

            importer.Dispose();

            //foreach (var mesh in totalMeshes)
            //{
            //    mesh.SetPath(path);
            //    mesh.fileID = Path.GetFileName(path);
            //}

            return new ModelData { rootObject = rootGO, meshObjects = meshObjects, cameras = cameras, lights = lights };
        }

        public static Mesh ProcessMesh(Assimp.Mesh mesh, Assimp.Scene scene, Assimp.Matrix4x4 transform, string path = "")
        {
            List<Vertex> vertices = new List<Vertex>();
            int[] indices = mesh.GetIndices();

            for (int i = 0; i < mesh.VertexCount; i++)
            {
                Vertex vertex;
                OpenTK.Mathematics.Vector3 modifiedVertex = (OpenTK.Mathematics.Vector4.TransformColumn(ToOpenTKMatrix(transform), new OpenTK.Mathematics.Vector4(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z, 1)) * Matrix4.CreateScale(0.02f)).Xyz;
                OpenTK.Mathematics.Vector3 modifiedNormals = (OpenTK.Mathematics.Vector4.TransformColumn(ToOpenTKMatrix(transform), new OpenTK.Mathematics.Vector4((mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z, 1))) * Matrix4.CreateScale(0.02f)).Xyz;
                if (mesh.TextureCoordinateChannels[0].Count != 0)
                {
                    modifiedNormals.Normalize();
                    if (mesh.Tangents.Count > 0 && mesh.BiTangents.Count > 0)
                    {
                        vertex = new Vertex(modifiedVertex, modifiedNormals, new OpenTK.Mathematics.Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y), new OpenTK.Mathematics.Vector3(mesh.Tangents[i].X, mesh.Tangents[i].Y, mesh.Tangents[i].Z), new OpenTK.Mathematics.Vector3(mesh.BiTangents[i].X, mesh.BiTangents[i].Y, mesh.BiTangents[i].Z));
                    }
                    else
                    {
                        vertex = new Vertex(modifiedVertex, modifiedNormals, new OpenTK.Mathematics.Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y));
                    }
                }
                else
                {
                    vertex = new Vertex(modifiedVertex, modifiedNormals, OpenTK.Mathematics.Vector2.Zero);
                }
                vertices.Add(vertex);
            }
            Mesh mesh1 = new Mesh();
            mesh1.name = mesh.Name;
            mesh1.MeshID = mesh.Name.GetHashCode();
            mesh1.SetPath(path);
            mesh1.SetVertices(vertices.ToArray());
            if (indices.Length != 0)
            {
                mesh1.SetIndices(indices);
            }

            uint materialID;

            if (processedMaterialIndices.ContainsKey(mesh.MaterialIndex))
            {
                materialID = processedMaterialIndices[mesh.MaterialIndex];
            }
            else
            {
                Core.Material material = SetupMaterial(mesh.MaterialIndex, scene, path);

                materialID = MaterialManager.AddMaterial(material);
                processedMaterialIndices[mesh.MaterialIndex] = materialID;
            }

            mesh1.MaterialIndex = materialID;
            return mesh1;
        }

        static Core.Material SetupMaterial(int matIndex, Assimp.Scene scene, string baseDir)
        {
            var assimpMat = scene.Materials[matIndex];
            var mat = Renderer3D.ActiveRenderingPipeline.GetDefaultMaterial();

            mat.Set("material.albedo", new OpenTK.Mathematics.Vector4(assimpMat.ColorDiffuse.R, assimpMat.ColorDiffuse.G, assimpMat.ColorDiffuse.B, assimpMat.ColorDiffuse.A));
            mat.Set("material.metallic", 0f);
            mat.Set("material.roughness", 1f - assimpMat.Reflectivity);
            mat.Set("material.emission", new OpenTK.Mathematics.Vector3(assimpMat.ColorEmissive.R, assimpMat.ColorEmissive.G, assimpMat.ColorEmissive.B));
            mat.Set("material.emissionStr", 1f);
            mat.Set("material.ambient", 0.1f);

            TrySetTexture(assimpMat, TextureType.Diffuse, "material.DIFFUSE_TEX", "material.useDiffuseMap", mat, baseDir);
            TrySetTexture(assimpMat, TextureType.Normals, "material.NORMAL_TEX", "material.useNormalMap", mat, baseDir);
            TrySetTexture(assimpMat, TextureType.Shininess, "material.ROUGHNESS_TEX", "material.useRoughnessMap", mat, baseDir);
            TrySetTexture(assimpMat, TextureType.Emissive, "material.EMISSIVE_TEX", "material.useEmissionMap", mat, baseDir);


            return mat;
        }
        static void TrySetTexture(Assimp.Material mat, TextureType type, string uniformName, string useFlag, Core.Material outputMat, string baseDir)
        {
            if (!mat.GetMaterialTexture(type, 0, out TextureSlot texSlot))
                return;

            string fullPath = Path.Combine(baseDir, texSlot.FilePath);

            // Load or get existing TextureReference
            TextureReference textureRef = (TextureReference)AssetLoader.Load<Texture>(fullPath);  // returns IAssetReference<Texture>

            if (textureRef == null)
                return;

            if (textureRef.isLoaded)
            {
                SetupTexture(textureRef.value, fullPath, texSlot, outputMat, uniformName, useFlag);
            }
            else
            {
                textureRef.OnLoad += (tex) =>
                {
                    SetupTexture(tex, fullPath, texSlot, outputMat, uniformName, useFlag);
                };
            }
        }

        static void SetupTexture(Texture texture, string path, TextureSlot texSlot, Core.Material mat, string uniformName, string useFlag)
        {

            texture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            texture.SetAnisotropy(8f);
            texture.GenerateMipmap();
            SetWrap(texSlot.WrapModeU, texSlot.WrapModeV, texture);

            mat.Set(uniformName, texture);
            mat.Set(useFlag, 1);
        }

        static Assimp.Matrix4x4 GetNodeWorldTransform(Node node)
        {
            Assimp.Matrix4x4 transform = node.Transform;

            Node current = node.Parent;
            while (current != null)
            {
                transform = current.Transform * transform;
                current = current.Parent;
            }

            return transform;
        }




        static GameObject FindGOByName(GameObject root, string name)
        {
            if (root.Name == name)
                return root;

            foreach (var child in root.children)
            {
                var found = FindGOByName(child, name);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static float ComputeLightRange(Light light, float threshold = 0.01f)
        {
            float Kc = light.AttenuationConstant;
            float Kl = light.AttenuationLinear;
            float Kq = light.AttenuationQuadratic;

            float intensityAtDistance = 1.0f / threshold;
            float c = Kc - intensityAtDistance;

            // Solve quadratic: Kq * d^2 + Kl * d + c = 0
            float discriminant = Kl * Kl - 4 * Kq * c;

            if (Kq == 0 || discriminant < 0)
                return 100.0f; // fallback if invalid or directional light

            float sqrtD = MathF.Sqrt(discriminant);
            float d1 = (-Kl + sqrtD) / (2 * Kq);
            float d2 = (-Kl - sqrtD) / (2 * Kq);

            float range = MathF.Max(d1, d2);
            return range > 0 ? range : 100.0f;
        }

        static void SetWrap(Assimp.TextureWrapMode U, Assimp.TextureWrapMode V, Texture texture)
        {
            if (U == Assimp.TextureWrapMode.Wrap && V == Assimp.TextureWrapMode.Wrap)
            {
                texture.SetWrapMode(Core.TextureWrapMode.Repeat, Core.TextureWrapMode.Repeat);
            }
            else if (U == Assimp.TextureWrapMode.Wrap && V == Assimp.TextureWrapMode.Clamp)
            {
                texture.SetWrapMode(Core.TextureWrapMode.Repeat, Core.TextureWrapMode.Clamp);
            }
            else if (U == Assimp.TextureWrapMode.Clamp && V == Assimp.TextureWrapMode.Wrap)
            {
                texture.SetWrapMode(Core.TextureWrapMode.Clamp, Core.TextureWrapMode.Repeat);
            }
        }

        static Core.Texture CheckTextureExists(string path)
        {
            return Textures.TryGetValue(path, out var tex) ? tex : null;
        }

        static void AddToTextureDict(string path, Core.Texture texture)
        {
            if (!Textures.ContainsKey(path))
            {
                Textures.Add(path, texture);
            }
        }

        public static Matrix4 ToOpenTKMatrix(Assimp.Matrix4x4 matrix)
        {
            return new Matrix4(matrix.A1, matrix.A2, matrix.A3, matrix.A4,
                               matrix.B1, matrix.B2, matrix.B3, matrix.B4,
                               matrix.C1, matrix.C2, matrix.C3, matrix.C4,
                               matrix.D1, matrix.D2, matrix.D3, matrix.D4);
        }


        static string CorrectFilePath(string path, string basePath = null)
        {
            if (IsFullPath(path))
            {
                return path;
            }
            return GetAbsolutePath(basePath, path);
        }

        public static bool IsFullPath(string path)
        {
            return !String.IsNullOrWhiteSpace(path)
                && path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) == -1
                && Path.IsPathRooted(path)
                && !Path.GetPathRoot(path).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
        }

        public static String GetAbsolutePath(String basePath, String path)
        {
            if (path == null)
                return null;
            if (basePath == null)
                basePath = Path.GetFullPath("."); // quick way of getting current working directory
            else
                basePath = GetAbsolutePath(null, basePath); // to be REALLY sure ;)
            String finalPath;
            // specific for windows paths starting on \ - they need the drive added to them.
            // I constructed this piece like this for possible Mono support.
            if (!Path.IsPathRooted(path) || "\\".Equals(Path.GetPathRoot(path)))
            {
                if (path.StartsWith(Path.DirectorySeparatorChar.ToString()))
                    finalPath = Path.Combine(Path.GetPathRoot(basePath), path.TrimStart(Path.DirectorySeparatorChar));
                else
                    finalPath = Path.Combine(basePath, path);
            }
            else
                finalPath = path;
            // resolves any internal "..\" to get the true full path.
            return Path.GetFullPath(finalPath);
        }
    }
}