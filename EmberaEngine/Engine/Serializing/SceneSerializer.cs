using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK.Mathematics;

namespace EmberaEngine.Engine.Serializing
{
    public class SceneSerializer
    {
        public static void Serialize(Scene scene)
        {
            var resolver = CompositeResolver.Create(
                new IMessagePackFormatter[] {
                new GameObjectFormatter(),
                new Vector2Formatter(),
                new Vector3Formatter(),
                new Vector4Formatter()
                        },
                        new IFormatterResolver[] {
                ContractlessStandardResolver.Instance // avoids private members!
                        }
            );

            var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);


            string sPrettyStr;
            var item2 = MessagePackSerializer.ConvertToJson(MessagePackSerializer.Serialize(scene, options));

            Console.WriteLine(item2);

            sPrettyStr = JToken.Parse(item2).ToString(Formatting.Indented);

            Console.WriteLine(sPrettyStr);
        }



    }


    public class GameObjectFormatter : IMessagePackFormatter<GameObject>
    {
        public void Serialize(ref MessagePackWriter writer, GameObject value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(4);

            writer.Write(value.Name);
            writer.Write(value.Id.ToString());
            writer.Write(value.parentObject?.Id.ToString() ?? ""); // Empty if root

            // Serialize children recursively
            options.Resolver.GetFormatterWithVerify<List<GameObject>>()
                .Serialize(ref writer, value.children, options);

            // Serialize components
            var components = value.GetComponents();
            writer.WriteArrayHeader(components.Count);
            foreach (var comp in components)
            {
                var id = ComponentRegistry.GetId(comp.GetType());
                writer.Write(id);
                var formatter = ComponentRegistry.GetFormatter(id);
                formatter.Serialize(ref writer, comp, options);
            }
        }

        public GameObject Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            reader.ReadArrayHeader();

            var name = reader.ReadString();
            var id = Guid.Parse(reader.ReadString());
            var parentIdStr = reader.ReadString();
            var parentId = string.IsNullOrEmpty(parentIdStr) ? (Guid?)null : Guid.Parse(parentIdStr);

            var children = options.Resolver.GetFormatterWithVerify<List<GameObject>>()
                .Deserialize(ref reader, options);

            var go = new GameObject
            {
                Name = name,
                Id = id,
                children = children
            };

            // Set parent references recursively
            foreach (var child in children)
            {
                child.parentObject = go;
            }

            var componentCount = reader.ReadArrayHeader();
            for (int i = 0; i < componentCount; i++)
            {
                var compId = reader.ReadUInt16();
                var formatter = ComponentRegistry.GetFormatter(compId);
                var comp = formatter.Deserialize(ref reader, options);
                go.AddComponent(comp);
            }

            return go;
        }
    }



    public class ComponentFormatter<T> : IMessagePackFormatter<T> where T : Component
    {
        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(3);
            writer.Write(value.Type);
            writer.Write(ComponentRegistry.GetId(typeof(T)));
            options.Resolver.GetFormatterWithVerify<T>().Serialize(ref writer, value, options);
        }

        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            reader.ReadArrayHeader();
            return options.Resolver.GetFormatterWithVerify<T>().Deserialize(ref reader, options);
        }
    }





    public class Vector2Formatter : IMessagePackFormatter<Vector2>
    {
        public void Serialize(ref MessagePackWriter writer, Vector2 value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(2);
            writer.Write(value.X);
            writer.Write(value.Y);
        }

        public Vector2 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var count = reader.ReadArrayHeader();
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            return new Vector2(x, y);
        }
    }

    public class Vector3Formatter : IMessagePackFormatter<Vector3>
    {
        public void Serialize(ref MessagePackWriter writer, Vector3 value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(3);
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }

        public Vector3 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var count = reader.ReadArrayHeader();
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();
            return new Vector3(x, y, z);
        }
    }

    public class Vector4Formatter : IMessagePackFormatter<Vector4>
    {
        public void Serialize(ref MessagePackWriter writer, Vector4 value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(4);
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            writer.Write(value.W);
        }

        public Vector4 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var count = reader.ReadArrayHeader();
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();
            var w = reader.ReadSingle();
            return new Vector4(x, y, z, w);
        }
    }

    public class Adapter<T> : IMessagePackFormatter<Component> where T : Component
    {
        private readonly IMessagePackFormatter<T> inner;

        public Adapter(IMessagePackFormatter<T> inner)
        {
            this.inner = inner;
        }

        public void Serialize(ref MessagePackWriter writer, Component value, MessagePackSerializerOptions options)
        {
            inner.Serialize(ref writer, (T)value, options);
        }

        public Component Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return inner.Deserialize(ref reader, options);
        }
    }



}
