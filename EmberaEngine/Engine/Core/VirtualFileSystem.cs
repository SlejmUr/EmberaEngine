using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Core
{
    public interface IAssetSource
    {
        bool Exists(string virtualPath);
        byte[] Open(string virtualPath);
        Stream OpenStream(string virtualPath);
        string ResolvePath(string virtualPath);
        IEnumerable<string> EnumerateCurrentLevel(string path);
        IEnumerable<string> EnumerateFiles(string path);
    }



    public static class VirtualFileSystem
    {
        private static readonly List<IAssetSource> _sources = new();

        public static void Mount(IAssetSource source)
        {
            _sources.Insert(0, source); // Highest priority first
        }

        public static bool Exists(string path)
            => _sources.Any(s => s.Exists(path));

        public static byte[] Open(string path)
        {
            foreach (var source in _sources)
                if (source.Exists(path))
                    return source.Open(path);

            throw new FileNotFoundException(path);
        }

        public static Stream OpenStream(string path)
        {
            foreach (var source in _sources)
                if (source.Exists(path))
                    return source.OpenStream(path);

            throw new FileNotFoundException(path);
        }

        public static string ResolvePath(string path) => _sources[0].ResolvePath(path);

        public static IEnumerable<string> EnumerateCurrentLevel(string path) => _sources.SelectMany(s => s.EnumerateCurrentLevel(path)).Distinct();

        public static IEnumerable<string> EnumerateAll(string path)
            => _sources.SelectMany(s => s.EnumerateFiles(path)).Distinct();

    }

    public class DirectoryAssetSource : IAssetSource
    {
        private readonly string _rootPath;

        public DirectoryAssetSource(string rootPath)
        {
            _rootPath = rootPath;
        }

        public bool Exists(string virtualPath)
            => File.Exists(Path.Combine(_rootPath, virtualPath));

        public byte[] Open(string virtualPath)
            => File.ReadAllBytes(Path.Combine(_rootPath, virtualPath));

        public Stream OpenStream(string virtualPath)
            => File.OpenRead(Path.Combine(_rootPath, virtualPath));

        public string ResolvePath(string virtualPath) => Path.Combine(_rootPath, virtualPath);

        public IEnumerable<string> EnumerateCurrentLevel(string path) => Directory.EnumerateFiles(Path.Combine(_rootPath, path), "*")
            .Select(p => Path.GetRelativePath(_rootPath, p).Replace("\\", "/"));

        public IEnumerable<string> EnumerateFiles(string path)
            => Directory.EnumerateFiles(Path.Combine(_rootPath, path), "*", SearchOption.AllDirectories)
                        .Select(p => Path.GetRelativePath(_rootPath, p).Replace("\\", "/"));
    }

}
