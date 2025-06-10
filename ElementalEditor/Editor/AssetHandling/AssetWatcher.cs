using EmberaEngine.Engine.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.AssetHandling
{
    class AssetWatcher
    {
        static FileSystemWatcher watcher;
        static string watcherRootDirectory;

        public static void SetupWatcher(string path)
        {
            watcherRootDirectory = path;
            watcher = new FileSystemWatcher(path);
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size;
            watcher.Filter = "*.*";
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;

            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Renamed += OnRenamed;
        }

        static readonly ConcurrentDictionary<string, DateTime> debounceMap = new();
        static readonly TimeSpan debounceTime = TimeSpan.FromMilliseconds(5000);

        static void OnChanged(object sender, FileSystemEventArgs e)
        {
            Debounce(e.FullPath);
        }

        static void OnRenamed(object sender, RenamedEventArgs e)
        {
            Debounce(e.FullPath);
        }

        static void Debounce(string path)
        {
            if (Directory.Exists(path))
                return; // ignore directory changes

            Task.Run(async () =>
            {
                await WaitUntilFileIsReady(path);

                Console.WriteLine($"Hot reloading asset at {path}");

                MainThreadDispatcher.Queue(() =>
                {
                    AssetReferenceRegistry.Reload(Path.GetRelativePath(watcherRootDirectory, path)); // This should handle resolving type + reloading
                });
            });
        }

        static async Task WaitUntilFileIsReady(string path, int timeoutMs = 5000)
        {
            var sw = Stopwatch.StartNew();
            while (!IsFileReady(path))
            {
                if (sw.ElapsedMilliseconds > timeoutMs)
                    throw new TimeoutException($"File not ready after {timeoutMs}ms: {path}");

                await Task.Delay(1000);
            }
        }


        static bool IsFileReady(string path)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return true;
                }
            }
            catch (IOException)
            {
                return false;
            }
        }


    }
}
