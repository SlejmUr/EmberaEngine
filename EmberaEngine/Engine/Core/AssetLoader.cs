using EmberaEngine.Engine.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Core
{
    public interface IAssetLoader
    {
        IEnumerable<string> SupportedExtensions { get; } // [".png", ".dds"]
    }

    public interface IAssetLoader<T> : IAssetLoader where T : class
    {
        public IAssetReference<T> Load(string virtualPath);
    }


    public static class AssetLoader
    {
        private static readonly Dictionary<Type, IAssetLoader> _loaders = new();


        static AssetLoader()
        {
            Register<Texture>(new TextureLoader());
        }

        public static void Register<T>(IAssetLoader<T> loader) where T : class
        {
            _loaders[typeof(T)] = loader;
        }

        public static IAssetReference<T> Load<T>(string virtualPath) where T : class
        {
            if (AssetCache.TryGet<T>(virtualPath, out var reference))
                return (IAssetReference<T>)reference;

            if (!_loaders.TryGetValue(typeof(T), out var loaderObj))
                throw new Exception($"No loader registered for type {typeof(T)}");

            var loader = (IAssetLoader<T>)loaderObj;
            var newRef = loader.Load(virtualPath);

            AssetCache.Add<T>(virtualPath, newRef);
            AssetReferenceRegistry.Register(PathUtils.NormalizeVirtualPath(virtualPath), newRef);
            Console.WriteLine("REGISTER: " + PathUtils.NormalizeVirtualPath(virtualPath));

            return newRef;
        }

        public static IAssetLoader<T> GetLoader<T>() where T : class
        {
            if (!_loaders.TryGetValue(typeof(T), out var loaderObj))
                throw new Exception($"No loader registered for type {typeof(T).FullName}");

            return (IAssetLoader<T>)loaderObj;
        }

        public static Type? GuessAssetType(string assetPath)
        {
            string ext = Path.GetExtension(assetPath).ToLowerInvariant();

            foreach (var kv in _loaders)
            {
                if (kv.Value.SupportedExtensions.Contains(ext))
                    return kv.Key;
            }

            return null;
        }
    }

    public class TextureLoader : IAssetLoader<Texture>
    {
        public IEnumerable<string> SupportedExtensions = [
            "png", "jpg", "jpeg", "tga", "exr"    
        ];

        IEnumerable<string> IAssetLoader.SupportedExtensions => SupportedExtensions;

        IAssetReference<Texture> IAssetLoader<Texture>.Load(string virtualPath)
        {
            var textureReference = new TextureReference();

            (int, int, int) imageDimensions = Image.GetImageDimensions(VirtualFileSystem.OpenStream(virtualPath));
            imageDimensions.Item3 = 4; // hardcoded value since im always returning with alpha added anyway
            uint imageSize = (uint)(imageDimensions.Item1 * imageDimensions.Item2 * imageDimensions.Item3);
            BufferObject<byte> stagingBuffer = new BufferObject<byte>(OpenTK.Graphics.OpenGL.BufferStorageTarget.PixelUnpackBuffer, imageSize, OpenTK.Graphics.OpenGL.BufferStorageFlags.MapPersistentBit | OpenTK.Graphics.OpenGL.BufferStorageFlags.MapWriteBit);

            unsafe
            {
                void* bufferMemory = stagingBuffer.GetMappedBufferRange(0, imageSize, OpenTK.Graphics.OpenGL.BufferAccessMask.MapPersistentBit | OpenTK.Graphics.OpenGL.BufferAccessMask.MapWriteBit);

                Task.Run(() =>
                {

                    byte[] file = VirtualFileSystem.Open(virtualPath);
                    var img = new Image();
                    img.Load(file);

                    unsafe
                    {
                        fixed (byte* sourcePtr = img.Pixels)
                        {
                            NativeMemory.Copy(sourcePtr, bufferMemory, imageSize);
                        }
                    }

                    MainThreadDispatcher.Queue(async () =>
                    {
                        var texture = new Texture(TextureTarget2d.Texture2D);
                        texture.TexImage2D(imageDimensions.Item1, imageDimensions.Item2, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
                        stagingBuffer.Bind(OpenTK.Graphics.OpenGL.BufferTarget.PixelUnpackBuffer);
                        texture.SubTexture2D(imageDimensions.Item1, imageDimensions.Item2, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
                        stagingBuffer.DeleteBuffer();
                        texture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
                        texture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
                        texture.GenerateMipmap();

                        textureReference.SetValue(texture); // calls OnLoad internally

                        img.Pixels = [];
                    });
                });

            }

            //Task.Run(async () =>
            //{
            //    await Task.Delay(UtilRandom.Next(10) * 1000);
            //    byte[] file = VirtualFileSystem.Open(virtualPath);
            //    var img = new Image();
            //    img.Load(file);

            //    MainThreadDispatcher.Queue(async () =>
            //    {
            //        var texture = new Texture(TextureTarget2d.Texture2D);
            //        texture.TexImage2D(imageDimensions.Item1, imageDimensions.Item2, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.UnsignedByte, img.Pixels);
            //        texture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            //        texture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            //        texture.GenerateMipmap();

            //        textureReference.SetValue(texture); // calls OnLoad internally

            //        img.Pixels = [];
            //    });
            //});

            return textureReference;
        }

    }
}
