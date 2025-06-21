using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace EmberaEngine.SourceGen
{
    [Generator]
    public class ComponentRegistryGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var componentTypes = context.CompilationProvider.Select(static (compilation, _) =>
            {
                return GetAllTypes(compilation.GlobalNamespace)
                    .Where(t => t.DeclaredAccessibility == Accessibility.Public && IsDerivedFromComponent(t))
                    .ToImmutableArray();
            });


            context.RegisterSourceOutput(componentTypes, GenerateComponentRegistryClass);
        }
        private void GenerateComponentRegistryClass(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> types)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using EmberaEngine.Engine.Components;");
            sb.AppendLine("using EmberaEngine.Engine.Utilities;");
            sb.AppendLine();
            sb.AppendLine("namespace EmberaEngine.Generated");
            sb.AppendLine("{");
            sb.AppendLine("    public static class ComponentGeneratedRegisterType");
            sb.AppendLine("    {");
            sb.AppendLine("        [ModuleInitializer]");
            sb.AppendLine("        public static void RegisterAll()");
            sb.AppendLine("        {");

            foreach (var type in types.Distinct(SymbolEqualityComparer.Default))
            {
                var fullTypeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                sb.AppendLine($"            ComponentRegistry.Register<{fullTypeName}>();");
            }

            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource("ComponentGeneratedRegisterType.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }




        private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns)
        {
            foreach (var member in ns.GetMembers())
            {
                if (member is INamespaceSymbol nestedNs)
                {
                    foreach (var type in GetAllTypes(nestedNs))
                    {
                        yield return type;
                    }
                } else if (member is INamedTypeSymbol type)
                {
                    yield return type;
                    foreach (var nestedType in GetNestedTypes(type))
                    {
                        yield return nestedType;
                    }
                }
            }
        }

        private static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol type)
        {
            foreach (var nestedType in type.GetTypeMembers())
            {
                yield return nestedType;
                foreach(var deeperNested in GetNestedTypes(nestedType))
                {
                    yield return deeperNested;
                }
            }
        }

        private static bool IsDerivedFromComponent(INamedTypeSymbol symbol)
        {
            var baseType = symbol.BaseType;
            while (baseType != null)
            {
                if (baseType.Name == "Component" && baseType.ContainingNamespace.ToDisplayString() == "EmberaEngine.Engine.Components")
                    return true;

                baseType = baseType.BaseType;
            }
            return false;
        }

    }


}
