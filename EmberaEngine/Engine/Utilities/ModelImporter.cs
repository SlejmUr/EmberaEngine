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
//using BepuPhysics.Collidables;
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
            public Mesh mesh;
            public Core.Material material;
        }


        static Dictionary<string, Core.Texture> Textures = new Dictionary<string, Core.Texture>();
        static Dictionary<int, uint> processedMaterialIndices = new Dictionary<int, uint>();
        public static Mesh[] LoadModel(string path)
        {
            var importer = new AssimpContext();
            Assimp.Scene scene;

            try
            {
                scene = importer.ImportFile(path,
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
                Console.WriteLine($"[ModelImporter] Failed to load model: {path}\nError: {e.Message}");
                return null;
            }

            if (scene?.RootNode == null) return null;

            List<Mesh> totalMeshes = new List<Mesh>();
            string baseDir = Path.GetDirectoryName(Path.GetFullPath(path)).Replace("\\", "/");
            int totalMeshCount = scene.MeshCount;
            int meshProcessed = 0;

            void ProcessNode(Node node)
            {
                foreach (int meshIdx in node.MeshIndices)
                {
                    var assimpMesh = scene.Meshes[meshIdx];
                    var processed = ProcessMesh(assimpMesh, scene, node.Transform, baseDir);
                    totalMeshes.Add(processed);
                    meshProcessed++;
                    Console.WriteLine($"[ModelImporter] Progress: {(meshProcessed / (float)totalMeshCount):P0}");
                }

                foreach (var child in node.Children)
                    ProcessNode(child);
            }

            ProcessNode(scene.RootNode);
            importer.Dispose();

            foreach (var mesh in totalMeshes)
            {
                mesh.SetPath(path);
                mesh.fileID = Path.GetFileName(path);
            }

            return totalMeshes.ToArray();
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
                    // (Matrix4.Mult( * new Vector4(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z, 1)).xyz;
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
                    vertex = new Vertex(modifiedVertex, modifiedNormals, OpenTK.Mathematics.Vector2.Zero);//, new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y));
                }
                // process vertex positions, normals and texture coordinates
                vertices.Add(vertex);
            }
            Mesh mesh1 = new Mesh();
            mesh1.name = mesh.Name;
            mesh1.MeshID = mesh.Name.GetHashCode();
            mesh1.SetVertices(vertices.ToArray());
            if (indices.Length != 0)
            {
                mesh1.SetIndices(indices);
            }

            uint materialID;

            if (processedMaterialIndices.ContainsKey(mesh.MaterialIndex))
            {
                materialID = processedMaterialIndices[mesh.MaterialIndex];
            } else
            {
                Core.Material material = SetupMaterial(mesh.MaterialIndex, scene, path);

                materialID = MaterialManager.AddMaterial(material);
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

            string fullPath = CorrectFilePath(texSlot.FilePath, baseDir);
            var texture = CheckTextureExists(fullPath) ?? Helper.loadImageAsTex(fullPath, TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);

            if (texture == null) return;

            texture.SetAnisotropy(8f);
            texture.GenerateMipmap();
            SetWrap(texSlot.WrapModeU, texSlot.WrapModeV, texture);
            AddToTextureDict(fullPath, texture);


            outputMat.Set(uniformName, texture);
            outputMat.Set(useFlag, 1);
        }


        static void SetWrap(Assimp.TextureWrapMode U, Assimp.TextureWrapMode V, Texture texture)
        {
            if (U == Assimp.TextureWrapMode.Wrap && V == Assimp.TextureWrapMode.Wrap) {
                texture.SetWrapMode(Core.TextureWrapMode.Repeat, Core.TextureWrapMode.Repeat);
            } else if (U == Assimp.TextureWrapMode.Wrap && V == Assimp.TextureWrapMode.Clamp)
            {
                texture.SetWrapMode(Core.TextureWrapMode.Repeat, Core.TextureWrapMode.Clamp);
            } else if (U == Assimp.TextureWrapMode.Clamp && V == Assimp.TextureWrapMode.Wrap)
            {
                texture.SetWrapMode(Core.TextureWrapMode.Clamp, Core.TextureWrapMode.Repeat);
            }

        }

        static Core.Texture CheckTextureExists(string path)
        {
            Core.Texture value;
            if (Textures.ContainsKey(path))
            {
                return Textures[path];
            }
            return null;
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
            return new Matrix4(matrix.A1, matrix.A2, matrix.A3, matrix.A4, matrix.B1, matrix.B2, matrix.B3, matrix.B4, matrix.C1, matrix.C2, matrix.C3, matrix.C4, matrix.D1, matrix.D2, matrix.D3, matrix.D4);
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