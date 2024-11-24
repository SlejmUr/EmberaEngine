using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using EmberaEngine.Engine.Utilities;

namespace EmberaEngine.Engine.Core
{
    public struct ShaderProperties
    {
        public bool requiresTime;
    }

    public class Shader
    {

        public static List<Shader> Shaders = new List<Shader>();

        public Dictionary<string, int> UniformPositions = new Dictionary<string, int>();

        private Dictionary<string, int> uniformInts = new Dictionary<string, int>();
        private Dictionary<string, float> uniformFloats = new Dictionary<string, float>();
        private Dictionary<string, bool> uniformBools = new Dictionary<string, bool>();
        private Dictionary<string, Vector2> uniformVec2 = new Dictionary<string, Vector2>();
        private Dictionary<string, Vector3> uniformVec3 = new Dictionary<string, Vector3>();
        private Dictionary<string, Vector4> uniformVec4 = new Dictionary<string, Vector4>();
        private Dictionary<string, Matrix3> uniformMat3 = new Dictionary<string, Matrix3>();
        private Dictionary<string, Matrix4> uniformMat4 = new Dictionary<string, Matrix4>();

        int Handle;

        string vPath, fPath;

        private bool disposedValue = false;

        public ShaderProperties ShaderProperties = new ShaderProperties();

        public Shader(string vertexPath, string fragmentPath, string geometryPath = "")
        {

            int VertexShader;
            int FragmentShader;
            int GeometryShader = 0;

            string VertexShaderSource;
            string FragmentShaderSource;
            string GeometryShaderSource;

            using (StreamReader reader = new StreamReader(vertexPath, Encoding.UTF8))
            {
                VertexShaderSource = reader.ReadToEnd();
            }

            using (StreamReader reader = new StreamReader(fragmentPath, Encoding.UTF8))
            {
                FragmentShaderSource = reader.ReadToEnd();
            }

            if (geometryPath != "")
            {
                using (StreamReader reader = new StreamReader(geometryPath, Encoding.UTF8))
                {
                    GeometryShaderSource = reader.ReadToEnd();
                    GeometryShader = GL.CreateShader(ShaderType.GeometryShader);
                    GL.ShaderSource(GeometryShader, GeometryShaderSource);
                }
            }

            vPath = vertexPath;
            fPath = fragmentPath;

            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            CompileSource(VertexShader, FragmentShader, geometryPath != "" ? GeometryShader : 0);
        }

        public Shader(string Path)
        {

            int VertexShader;
            int FragmentShader;

            string VertexShaderSource;
            string FragmentShaderSource;

            using (StreamReader reader = new StreamReader(Path + ".vert", Encoding.UTF8))
            {
                VertexShaderSource = reader.ReadToEnd();
            }

            using (StreamReader reader = new StreamReader(Path + ".frag", Encoding.UTF8))
            {
                FragmentShaderSource = reader.ReadToEnd();
            }

            vPath = Path + ".vert";
            fPath = Path + ".frag";

            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            CompileSource(VertexShader, FragmentShader);
        }

        public Shader(string VertexShaderSource, string FragmentShaderSource)
        {

            int VertexShader;
            int FragmentShader;

            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            CompileSource(VertexShader, FragmentShader);
        }

        public void CompileSource(int handleV, int handleF, int handleG = 0)
        {
            // Compiling the shaders
            GL.CompileShader(handleV);
            GL.CompileShader(handleF);
            if (handleG != 0)
                GL.CompileShader(handleG);

            // Getting Shader logs and printing
            string infoVertLog = GL.GetShaderInfoLog(handleV);
            string infoFragLog = GL.GetShaderInfoLog(handleF);
            if (infoVertLog != System.String.Empty) System.Console.WriteLine(infoVertLog);
            if (infoFragLog != System.String.Empty) System.Console.WriteLine(infoFragLog);

            // Creating Shader Program
            Handle = GL.CreateProgram();

            // Attaching Frag and Vert shader to program
            GL.AttachShader(Handle, handleV);
            GL.AttachShader(Handle, handleF);
            if (handleG != 0)
                GL.AttachShader(Handle, handleG);

            GL.LinkProgram(Handle);

            // Discarding Useless Resources
            GL.DetachShader(Handle, handleV);
            GL.DetachShader(Handle, handleF);
            if (handleG != 0)
                GL.DetachShader(Handle, handleG);
            GL.DeleteShader(handleV);
            GL.DeleteShader(handleF);
            if (handleG != 0)
                GL.DeleteShader(handleG);
            Shaders.Add(this);

            ShaderProperties.requiresTime = UniformExists("E_TIME");
        }

        public bool ExistsInAssetDB(string path)
        {
            Shader shader = AssetDatabase.Get(System.IO.Path.GetFileNameWithoutExtension(path));
            if (shader != null)
            {
                return true;
            }
            return false;
        }


        public void SetInt(string name, int value)
        {
            int location = this.GetUniformLocation(name);
            GL.Uniform1(location, value);
        }

        public void SetIntArray(string name, int[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                int location = this.GetUniformLocation(name + "["+i+"]");
                GL.Uniform1(location, value[i]);
            }
        }

        public void SetFloat(string name, float value)
        {
            int location = this.GetUniformLocation(name);
            GL.Uniform1(location, value);
        }

        public void SetBool(string name, bool value)
        {
            int location = this.GetUniformLocation(name);
            GL.Uniform1(location, value ? 1 : 0);
        }

        public void SetMatrix4(string name, Matrix4 value, bool transpose = false)
        {
            int location = this.GetUniformLocation(name);
            GL.UniformMatrix4(location, transpose, ref value);
        }

        public void SetVector4(string name, Vector4 value)
        {
            int location = this.GetUniformLocation(name);
            GL.Uniform4(location, value);
        }

        public void SetVector3(string name, Vector3 value)
        {
            int location = this.GetUniformLocation(name);
            GL.Uniform3(location, value);
        }

        public void SetVector2(string name, Vector2 value)
        {
            int location = this.GetUniformLocation(name);
            GL.Uniform2(location, value);
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public int GetRendererID()
        {
            return Handle;
        }

        public void ReCompile()
        {
            //Console.WriteLine("Recompiling: ", Handle);
            int VertexShader;
            int FragmentShader;

            string VertexShaderSource;
            string FragmentShaderSource;

            using (StreamReader reader = new StreamReader(vPath, Encoding.UTF8))
            {
                VertexShaderSource = reader.ReadToEnd();
            }

            using (StreamReader reader = new StreamReader(fPath, Encoding.UTF8))
            {
                FragmentShaderSource = reader.ReadToEnd();
            }

            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            Shaders.Remove(this);
            CompileSource(VertexShader, FragmentShader);
        }

        public int GetAttribLocation(string AttribName)
        {
            return GL.GetAttribLocation(Handle, AttribName);
        }

        public int GetUniformLocation(string UniformName)
        {
            if (UniformPositions.ContainsKey(UniformName))
            {
                return UniformPositions[UniformName];
            } else
            {
                UniformPositions.Add(UniformName, GL.GetUniformLocation(Handle, UniformName));
            }
            return GL.GetUniformLocation(Handle, UniformName);
        }

        public bool UniformExists(string name)
        {
            if (GetUniformLocation(name) != -1)
                return true;
            return false;
        }

        public void Set(string name, int value)
        {
            uniformInts[name] = value;
        }
        public void Set(string name, float value)
        {
            uniformFloats[name] = value;
        }
        public void Set(string name, bool value)
        {
            uniformBools[name] = value;
        }
        public void Set(string name, Vector2 value)
        {
            uniformVec2[name] = value;
        }
        public void Set(string name, Vector3 value)
        {
            uniformVec3[name] = value;
        }
        public void Set(string name, Vector4 value)
        {
            uniformVec4[name] = value;
        }
        public void Set(string name, Matrix3 value)
        {
            uniformMat3[name] = value;
        }
        public void Set(string name, Matrix4 value)
        {
            uniformMat4[name] = value;
        }

        public void Apply()
        {
            foreach (string key in uniformInts.Keys)
            {
                SetInt(key, uniformInts[key]);
            }
            foreach (string key in uniformFloats.Keys)
            {
                SetFloat(key, uniformFloats[key]);
            }
            foreach (string key in uniformBools.Keys)
            {
                SetBool(key, uniformBools[key]);
            }
            foreach (string key in uniformVec2.Keys)
            {
                SetVector2(key, uniformVec2[key]);
            }
            foreach (string key in uniformVec3.Keys)
            {
                SetVector3(key, uniformVec3[key]);
            }
            foreach (string key in uniformVec4.Keys)
            {
                SetVector4(key, uniformVec4[key]);
            }
            foreach (string key in uniformMat4.Keys)
            {
                SetMatrix4(key, uniformMat4[key]);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);
                disposedValue = true;
            }
        }



        ~Shader()
        {
            if (!GL.IsProgram(Handle)) { return; }
            GL.DeleteProgram(Handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
