using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using EmberaEngine.Engine.Utilities;

namespace EmberaEngine.Engine.Core
{
    public class ComputeShader : IDisposable
    {


        public Dictionary<string, int> UniformPositions = new Dictionary<string, int>();

        int Handle;

        Texture OutputTexture;

        Vector3i WorkGroupSize;

        string Cpath;

        private bool disposedValue = false;

        public ComputeShader(string path, Vector3i? WorkGroupSize = null)
        {
            Cpath = path;

            int computeShader;

            string ComputeShaderSource;

            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                ComputeShaderSource = reader.ReadToEnd();
            }

            computeShader = GL.CreateShader(ShaderType.ComputeShader);
            GL.ShaderSource(computeShader, ComputeShaderSource);

            CompileSource(computeShader);

            this.WorkGroupSize = WorkGroupSize ?? Vector3i.Zero;
        }

        public void CreateOutputTexture()
        {

            OutputTexture = new Texture(TextureTarget2d.Texture2D);
            OutputTexture.TexImage2D(WorkGroupSize.X, WorkGroupSize.Y, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            OutputTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        }

        public int GetOutputRendererID()
        {
            return OutputTexture.GetRendererID();
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void Dispatch()
        {
            GL.DispatchCompute(WorkGroupSize.X, WorkGroupSize.Y, WorkGroupSize.Z);
        }

        public void Dispatch(int workGroupX, int WorkGroupY, int workGroupZ)
        {
            GL.DispatchCompute(workGroupX, WorkGroupY, workGroupZ);
        }

        public void Wait()
        {
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
        }

        public void CompileSource(int handle)
        {
            // Compiling the shaders
            GL.CompileShader(handle);

            // Getting Shader logs and printing
            string infoLog = GL.GetShaderInfoLog(handle);
            if (infoLog != System.String.Empty) System.Console.WriteLine(infoLog);

            // Creating Shader Program
            Handle = GL.CreateProgram();

            // Attaching Frag and Vert shader to program
            GL.AttachShader(Handle, handle);

            GL.LinkProgram(Handle);

            // Discarding Useless Resources
            GL.DetachShader(Handle, handle);
            GL.DeleteShader(handle);
        }

        public void ReCompile()
        {
            string ComputeShaderSource;

            using (StreamReader reader = new StreamReader(Cpath, Encoding.UTF8))
            {
                ComputeShaderSource = reader.ReadToEnd();
            }

            Handle = GL.CreateShader(ShaderType.ComputeShader);
            GL.ShaderSource(Handle, ComputeShaderSource);
            CompileSource(Handle);
        }

        //public void SetFloatArray(float[] values)
        //{
        //    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, WorkGroupSize.X, WorkGroupSize.Y, 0, PixelFormat.Red, PixelType.Float, values);
        //}

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

        public void SetVector3ui(string name, Vector3i value)
        {
            int location = this.GetUniformLocation(name);
            GL.Uniform3(location, (uint)value.X, (uint)value.Y, (uint)value.Z);
        }

        public void SetVector2(string name, Vector2 value)
        {
            int location = this.GetUniformLocation(name);
            GL.Uniform2(location, value);
        }

        public void SetVector2ui(string name, Vector2i value)
        {
            int location = this.GetUniformLocation(name);
            GL.Uniform2(location, (uint)value.X, (uint)value.Y);
        }

        public void SetInt(string name, int value)
        {
            int location = this.GetUniformLocation(name);
            GL.Uniform1(location, value);
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
            }
            else
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);
                disposedValue = true;

                if (OutputTexture != null)
                {
                    GraphicsObjectCollector.AddTexToDispose(OutputTexture.GetRendererID());
                    OutputTexture = null;
                }

            }
        }

        ~ComputeShader()
        {
            //if (!GL.IsProgram(Handle)) { return; }
            //GL.DeleteProgram(Handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}