using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using SharpFont;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EmberaEngine.Engine.Utilities
{
    public unsafe class BufferObject<T> where T : unmanaged
    {

        int handle;
        BufferStorageTarget target;

        public BufferObject(BufferStorageTarget target, T data, BufferUsageHint bufferUsageHint = BufferUsageHint.StaticCopy)
        {
            this.target = target;
            GL.CreateBuffers(1, out handle);


            int size = System.Runtime.InteropServices.Marshal.SizeOf<T>();

            //Console.WriteLine("Struct Size: " + size);
            

            GL.NamedBufferData(handle, size, ref data, bufferUsageHint);

        }

        public BufferObject(BufferStorageTarget target, uint size, T data, BufferUsageHint bufferUsageHint = BufferUsageHint.StaticCopy)
        {
            this.target = target;
            GL.CreateBuffers(1, out handle);

            //Console.WriteLine("Struct Size: " + size);

            GL.NamedBufferData(handle, (int)size, ref data, bufferUsageHint);
        }

        public BufferObject(BufferStorageTarget target, T[] data, BufferUsageHint bufferUsageHint = BufferUsageHint.StaticCopy)
        {
            this.target = target;
            GL.CreateBuffers(1, out handle);

            int size = sizeof(T) * data.Length;

            fixed (T* ptr = &data[0])
            {
                GL.NamedBufferData(handle, size, (IntPtr)ptr, bufferUsageHint);
            }
        }

        public BufferObject(BufferStorageTarget target, uint size, BufferUsageHint bufferUsageHint = BufferUsageHint.StaticCopy)
        {
            this.target = target;
            GL.CreateBuffers(1, out handle);
            GL.NamedBufferData(handle, (int)size, IntPtr.Zero, bufferUsageHint);
        }

        public BufferObject(BufferStorageTarget target, uint size, BufferStorageFlags flags = BufferStorageFlags.None)
        {
            this.target = target;
            GL.CreateBuffers(1, out handle);
            GL.NamedBufferStorage(handle, (int)size, IntPtr.Zero, flags);
        }

        public void* GetMappedBufferRange(int offset, uint size, BufferAccessMask bufferAccessMask)
        {
            return (void*)GL.MapNamedBufferRange(handle, offset, (int)size, bufferAccessMask);
        }

        public void SetData<T>(int offset, int size, in T data) where T : unmanaged
        {
            fixed (void* ptr = &data)
            {
                UploadData(offset, size, ptr);
            }
        }

        public void SetData<T>(int offset, int size, in T[] data) where T : unmanaged
        {
            fixed (void* ptr = &data[0])
            {
                UploadData(offset, size, ptr);
            }
        }

        public void UploadData(nint offset, nint size, void* data)
        {
            if (size == 0) return;
            GL.NamedBufferSubData(handle, offset, (int)size, (nint)data);
        }

        public void Bind(int index = 0)
        {
            GL.BindBufferBase((BufferRangeTarget)target, index, handle);
        }

        public void Bind(BufferTarget target)
        {
            GL.BindBuffer(target, handle);
        }

        public void DeleteBuffer()
        {
            GL.DeleteBuffer(handle);
        }


        public int GetRendererID()
        {
            return handle;
        }
    }


}
