using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace EmberaEngine.Engine.Core
{
    public struct ShaderProperties
    {
        public bool requiresTime;
    }

    public class Shader : IDisposable
    {
        public static List<Shader> Shaders = new List<Shader>();
        public static int CurrentlyBound = -1;
        public Dictionary<string, int> UniformPositions = new Dictionary<string, int>();

        private int Handle;
        private string vPath, fPath;
        private bool disposedValue = false;
        

        public ShaderProperties ShaderProperties = new ShaderProperties();

        public Shader(string vertexPath, string fragmentPath, string geometryPath = "")
        {
            int vertexShader = LoadShader(vertexPath, ShaderType.VertexShader, out string vertexSource);
            int fragmentShader = LoadShader(fragmentPath, ShaderType.FragmentShader, out string fragmentSource);
            int geometryShader = 0;

            if (!string.IsNullOrEmpty(geometryPath))
                geometryShader = LoadShader(geometryPath, ShaderType.GeometryShader, out _);

            vPath = vertexPath;
            fPath = fragmentPath;

            CompileSource(vertexShader, fragmentShader, geometryShader);
        }

        public Shader(string path)
            : this(path + ".vert", path + ".frag") { }

        private int LoadShader(string path, ShaderType type, out string source)
        {
            source = File.ReadAllText(path, Encoding.UTF8);
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            return shader;
        }

        private void CompileSource(int vs, int fs, int gs = 0)
        {
            GL.CompileShader(vs);
            GL.CompileShader(fs);
            if (gs != 0) GL.CompileShader(gs);

            LogShaderCompilation(vs);
            LogShaderCompilation(fs);
            if (gs != 0) LogShaderCompilation(gs);

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vs);
            GL.AttachShader(Handle, fs);
            if (gs != 0) GL.AttachShader(Handle, gs);

            GL.LinkProgram(Handle);

            GL.DetachShader(Handle, vs);
            GL.DetachShader(Handle, fs);
            if (gs != 0) GL.DetachShader(Handle, gs);

            GL.DeleteShader(vs);
            GL.DeleteShader(fs);
            if (gs != 0) GL.DeleteShader(gs);

            Shaders.Add(this);
            ShaderProperties.requiresTime = UniformExists("E_TIME");
        }

        private void LogShaderCompilation(int shader)
        {
            string log = GL.GetShaderInfoLog(shader);
            if (!string.IsNullOrEmpty(log))
                Console.WriteLine(log);
        }

        public void Use()
        {
            if (CurrentlyBound == Handle) return;
            GL.UseProgram(Handle);

            CurrentlyBound = Handle;
        }
        public int GetRendererID() => Handle;

        public void ReCompile()
        {
            int vs = LoadShader(vPath, ShaderType.VertexShader, out _);
            int fs = LoadShader(fPath, ShaderType.FragmentShader, out _);

            Shaders.Remove(this);
            CompileSource(vs, fs);
        }

        public int GetAttribLocation(string name) => GL.GetAttribLocation(Handle, name);

        public int GetUniformLocation(string name)
        {
            if (!UniformPositions.TryGetValue(name, out int location))
            {
                location = GL.GetUniformLocation(Handle, name);
                UniformPositions[name] = location;
            }
            return location;
        }

        public bool UniformExists(string name) => GetUniformLocation(name) != -1;

        // Uniform Setters
        public void Set(string name, object value)
        {
            switch (value)
            {
                case int i:
                    SetInt(name, i);
                    break;
                case float f:
                    SetFloat(name, f);
                    break;
                case bool b:
                    SetBool(name, b);
                    break;
                case Vector2 v2:
                    SetVector2(name, v2);
                    break;
                case Vector3 v3:
                    SetVector3(name, v3);
                    break;
                case Vector4 v4:
                    SetVector4(name, v4);
                    break;
                case Matrix4 m4:
                    SetMatrix4(name, m4);
                    break;
                default:
                    throw new ArgumentException($"Unsupported uniform type: {value.GetType().Name}", nameof(value));
            }
        }



        public void SetInt(string name, int value)
        {
            int location = GetUniformLocation(name);
            if (location != -1) GL.Uniform1(location, value);
        }

        public void SetIntArray(string name, int[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                int location = GetUniformLocation($"{name}[{i}]");
                if (location != -1) GL.Uniform1(location, value[i]);
            }
        }

        public void SetFloat(string name, float value)
        {
            int location = GetUniformLocation(name);
            if (location != -1) GL.Uniform1(location, value);
        }

        public void SetBool(string name, bool value) => SetInt(name, value ? 1 : 0);

        public void SetVector2(string name, Vector2 value)
        {
            int location = GetUniformLocation(name);
            if (location != -1) GL.Uniform2(location, value);
        }

        public void SetVector3(string name, Vector3 value)
        {
            int location = GetUniformLocation(name);
            if (location != -1) GL.Uniform3(location, value);
        }

        public void SetVector4(string name, Vector4 value)
        {
            int location = GetUniformLocation(name);
            if (location != -1) GL.Uniform4(location, value);
        }

        public void SetMatrix4(string name, Matrix4 value, bool transpose = false)
        {
            int location = GetUniformLocation(name);
            if (location != -1) GL.UniformMatrix4(location, transpose, ref value);
        }

        // Disposal
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
            if (GL.IsProgram(Handle))
                GL.DeleteProgram(Handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
