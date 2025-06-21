using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using OpenTK.Mathematics;

namespace EmberaEngine.Engine.Utilities
{
    public class SceneSerializer
    {
        public static void Serialize(Scene scene)
        {
            var resolver = CompositeResolver.Create(
                new IMessagePackFormatter[] {
                new GameObjectFormatter(),
                new Vector2Formatter()
                        },
                        new IFormatterResolver[] {
                ContractlessStandardResolver.Instance // avoids private members!
                        }
            );

            var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);

            Console.WriteLine(MessagePackSerializer.ConvertToJson(MessagePackSerializer.Serialize(scene, options)));
        }



    }


    public class GameObjectFormatter : IMessagePackFormatter<GameObject>
    {
        public void Serialize(ref MessagePackWriter writer, GameObject value, MessagePackSerializerOptions options)
        {
            Console.WriteLine($"Serializing GameObject: {value.Name}");

            writer.WriteArrayHeader(5); // Added 1 for components

            writer.Write(value.Name);
            writer.Write(value.Id.ToString());
            writer.Write(value.parentObject?.Id.ToString());

            // Serialize children (List<GameObject>)
            options.Resolver.GetFormatterWithVerify<List<GameObject>>()
                .Serialize(ref writer, value.children, options);

            // Get only the public components list
            var components = value.GetComponents(); // List<Component>

            // Serialize components list, each Component will be resolved by TypelessContractlessStandardResolver
            // But if you want to avoid serializing private fields, use ContractlessStandardResolver instead
            MessagePackSerializer.Serialize(ref writer, components, options);
        }

        public GameObject Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var count = reader.ReadArrayHeader();

            var name = reader.ReadString();
            var idStr = reader.ReadString();
            var parentId = reader.ReadString();
            var children = options.Resolver.GetFormatterWithVerify<List<GameObject>>()
                .Deserialize(ref reader, options);

            var components = MessagePackSerializer.Deserialize<List<Component>>(ref reader, options);

            var go = new GameObject
            {
                Name = name,
                Id = Guid.Parse(idStr),
                children = children
            };

            return go;
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


}
