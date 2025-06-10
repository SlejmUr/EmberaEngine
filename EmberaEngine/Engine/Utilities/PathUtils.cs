using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Utilities
{
    public static class PathUtils
    {
        public static string NormalizeVirtualPath(string path)
        {
            return path.Replace('\\', '/').TrimStart('/').TrimEnd('/');
        }


    }
}
