using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Serializing;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Utilities
{
    public static class ComponentRegistry
    {
        private static readonly Dictionary<ushort, Func<IMessagePackFormatter<Component>>> _idToFormatter = new();
        private static readonly Dictionary<Type, ushort> _typeToId = new();
        private static ushort _nextId = 0;

        public static void Register<T>() where T : Component, new()
        {
            var id = _nextId++;
            _typeToId[typeof(T)] = id;

            var typedFormatter = new ComponentFormatter<T>();
            _idToFormatter[id] = () => new Adapter<T>(typedFormatter); // ✅ Explicit cast via adapter
        }


        public static ushort GetId(Type type) => _typeToId[type];

        public static IMessagePackFormatter<Component> GetFormatter(ushort id) => _idToFormatter[id]();
    }


}
