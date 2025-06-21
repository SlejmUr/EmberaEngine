using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MessagePack;

namespace EmberaEngine.Engine.Core
{
    public class SceneUtils
    {

        public static void Save(Scene scene)
        {
            var options = MessagePackSerializerOptions.Standard.WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);
            byte[] data = MessagePackSerializer.Serialize(scene, options);
        }






    }
}
