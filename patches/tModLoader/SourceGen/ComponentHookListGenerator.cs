using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceGen
{
	[Generator]
	public class ComponentHookListGenerator : ISourceGenerator
	{
		/// <summary> Created on demand before each generation pass </summary>
		private class SyntaxReceiver : ISyntaxContextReceiver
		{
			public List<INamedTypeSymbol> Interfaces { get; } = new();

			/// <summary> Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation </summary>
			public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
				if (context.Node is not InterfaceDeclarationSyntax interfaceDeclarationSyntax || interfaceDeclarationSyntax.AttributeLists.Count <= 0) {
					return;
				}

				INamedTypeSymbol namedTypeSymbol = context.SemanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax);

				if (namedTypeSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == AttributeFullName)) {
					Interfaces.Add(namedTypeSymbol);
				}
			}
		}

		private const string AttributeNamespace = "Terraria.ModLoader";
		private const string AttributeName = "ComponentHookAttribute";
		private const string AttributeFullName = $"{AttributeNamespace}.{AttributeName}";
		private const string AttributeCode =
$@"using System;
			
namespace {AttributeNamespace}
{{
	[AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
	public sealed class {AttributeName} : Attribute
	{{

	}}
}}";

		public void Initialize(GeneratorInitializationContext context) {
			// Register the attribute source
			context.RegisterForPostInitialization((i) => i.AddSource(AttributeName, AttributeCode));

			// Register a syntax receiver that will be created for each generation pass
			context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
		}

		public void Execute(GeneratorExecutionContext context) {
			// Retrieve the populated receiver 
			if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
				return;

			var attributeSymbol = context.Compilation.GetTypeByMetadataName(AttributeFullName); // Get the added attribute

			// Process methods
			foreach (var interfaceType in receiver.Interfaces) {
				if (!interfaceType.ContainingSymbol.Equals(interfaceType.ContainingNamespace, SymbolEqualityComparer.Default)) {
					continue; //TODO: Issue a diagnostic that it must be top level
				}

				string interfaceName = interfaceType.Name;
				string namespaceName = interfaceType.ContainingNamespace.ToDisplayString();

				// Begin building the generated source
				var source = new StringBuilder();

				source.AppendLine($"namespace {namespaceName}");
				source.AppendLine($"{{");
				source.AppendLine($"\tpartial {interfaceType.TypeKind.ToString().ToLower()} {interfaceName}");
				source.AppendLine($"\t{{");

				var methods = interfaceType.GetMembers()
					.Select(m => m as IMethodSymbol)
					.Where(m => m != null && !m.IsStatic)
					.ToArray();

				foreach (var method in methods) {
					string memberSuffix = methods.Length > 1 ? method.Name : string.Empty;
					string methodName = method.Name;
					string delegateName = $"Delegate{memberSuffix}";
					string parameterCode = string.Join(", ", method.Parameters.Select(p => $"{p.ToDisplayString()} {p.Name}").Prepend($"Component component"));

					source.AppendLine($"\t\tprivate delegate {method.ReturnType.ToDisplayString()} {delegateName}({parameterCode});");

					source.AppendLine();

					string fieldName = $"Hook{memberSuffix}";
					string fieldType = $"ComponentHookList<{delegateName}>";
					string getMethodInfoCode = $@"typeof({interfaceName}).GetMethod(""{methodName}"")";

					source.AppendLine($"\t\tprivate static readonly {fieldType} {fieldName} = new({getMethodInfoCode});");
				}

				source.AppendLine($"\t}}");
				source.AppendLine($"}}");

				//string classSource = ProcessClass(namedParent, method.ToList(), attributeSymbol, notifySymbol, context);

				context.AddSource($"{interfaceName}.HookLists.cs", SourceText.From(source.ToString(), Encoding.UTF8));
			}
		}
	}
}
