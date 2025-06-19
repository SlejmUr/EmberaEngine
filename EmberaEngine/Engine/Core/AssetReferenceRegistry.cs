using EmberaEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Core
{

    public static class AssetReferenceRegistry
    {
        static Dictionary<string, (Type type, object reference)> references = new Dictionary<string, (Type type, object reference)>();

        public static void Register<T>(string path, IAssetReference<T> reference) where T : class
        {
            references.Add(path, (typeof(T), reference));
        }

        public static void Reload(string path)
        {
            path = PathUtils.NormalizeVirtualPath(path);

            if (!references.TryGetValue(path, out var entry))
                return;

            Type type = entry.type;

            var method = typeof(AssetReferenceRegistry)
                .GetMethod(nameof(ReloadGeneric), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(type);

            method.Invoke(null, new object[] { path, entry.reference });
        }

        private static void ReloadGeneric<T>(string path, object referenceObj) where T : class
        {
            var reference = (IAssetReference<T>)referenceObj;

            var loader = AssetLoader.GetLoader<T>(); // your system
            var newReference = loader.Load(path);

            newReference.OnLoad += (T value) => { reference.SetValue(value); };

        }
    }
}
