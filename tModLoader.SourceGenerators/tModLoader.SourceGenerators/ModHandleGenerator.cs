﻿using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static tModLoader.SourceGenerators.Constants;

namespace tModLoader.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public sealed class ModHandleGenerator : IIncrementalGenerator
{
	private const string Template_Namespace = "{Namespace}";
	private const string Template_DeclarationName = "{DeclarationName}";
	private const string Template_IdValue = "{IdValue}";
	private const string Template_HandleImplementation = "{HandleImplementation}";

	private const string TemplateCode_Mod = $@"// <auto-generated/>
using System.IO;
using System.Runtime.CompilerServices;

namespace {Template_Namespace};

partial class {Template_DeclarationName} {{
	[CompilerGenerated]
	public override void HandlePacket(BinaryReader reader, int whoAmI) {{
		byte type = reader.ReadByte();
{Template_HandleImplementation}
	}}
}}
";

	private const string TemplateCode_Packet = $@"// <auto-generated/>
using System.Runtime.CompilerServices;

namespace {Template_Namespace};

{Template_DeclarationName} {{
	[CompilerGenerated]
	public const byte Id = {Template_IdValue};
}}
";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// Commons.AssignDebugger();

		var modClassData = context.SyntaxProvider.CreateSyntaxProvider(
			static (node, _) => node is ClassDeclarationSyntax classDeclarationSyntax
				&& !classDeclarationSyntax.Modifiers.Any(SyntaxKind.AbstractKeyword)
				&& !classDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword)
				&& classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword)
				&& classDeclarationSyntax.BaseList != null && classDeclarationSyntax.BaseList.Types.Any(),
			static (ctx, token) => {
				var type = (ITypeSymbol)ctx.SemanticModel.GetDeclaredSymbol(ctx.Node, token);

				if (type.IsAbstract)
					return default;

				var baseType = type.BaseType;
				while (baseType != null && baseType.ToDisplayString() != "Terraria.ModLoader.Mod") {
					baseType = baseType.BaseType;
				}

				if (baseType != null) {
					return (
						Namespace: type.ContainingNamespace.ToString(),
						type.Name,
						FullyQualifiedName: type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
					);
				}

				return default;
			}).Where(static x => x.Namespace != null);

		var netPackets = context.SyntaxProvider.ForAttributeWithMetadataName(
			NetPacketAttributeFullMetadataName,
			NetPacketGeneratorv2.MatchStructAndRecordStruct,
			static (ctx, _) => {
				return (
					ctx.TargetSymbol.Name,
					DeclarationName: NetPacketGeneratorv2.GenerateDeclarationName((ITypeSymbol)ctx.TargetSymbol),
					Namespace: ctx.TargetSymbol.ContainingNamespace.ToString(),
					Token: ctx.TargetSymbol.MetadataToken,
					ModFullyQualifiedName: ((ISymbol)ctx.Attributes[0].ConstructorArguments[0].Value).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
				);
			})
			.Collect()
			.SelectMany(static (x, _) => x.OrderBy(x => x.Token).Select((x, i) => (PacketData: x, Id: i)));

		context.RegisterSourceOutput(modClassData.Combine(netPackets.Collect()), static (ctx, tuple) => {
			var modClassData = tuple.Left;
			var netPackets = tuple.Right;

			ctx.AddSource($"{modClassData.Namespace}.{modClassData.Name}.g.cs", MatchBrackets.Replace(TemplateCode_Mod, match => match.Value switch {
				Template_DeclarationName => modClassData.Name,
				Template_Namespace => modClassData.Namespace,
				Template_HandleImplementation => Create_HandleImplementation(),
				_ => match.Value
			}));

			string Create_HandleImplementation()
			{
				var writer = new IndentedStringBuilder();
				writer.Indent += 2;

				writer.WriteLine();
				writer.WriteLine("switch (type) {");
				writer.Indent++;

				foreach (var packet in netPackets) {
					writer.WriteLine($"case global::{packet.PacketData.Namespace}.{packet.PacketData.Name}.Id: {{");
					writer.Indent++;

					writer.WriteLine($"var packet = default(global::{packet.PacketData.Namespace}.{packet.PacketData.Name});");
					writer.WriteLine("packet.Receive(reader, whoAmI);");
					writer.WriteLine("break;");

					writer.Indent--;
					writer.WriteLine('}');
				}

				writer.WriteLine("default:");
				writer.Indent++;

				writer.WriteLine($"Logger.WarnFormat(\"{modClassData.Name}: Unknown Packet Type: {{0}}\", type.ToString());");
				writer.WriteLine("break;");
				writer.Indent--;

				writer.Indent--;
				writer.Write('}');

				return writer.ToString();
			}
		});

		context.RegisterSourceOutput(netPackets, static (ctx, packet) => {
			ctx.AddSource($"{packet.PacketData.Namespace}.{packet.PacketData.Name}.g.cs", MatchBrackets.Replace(TemplateCode_Packet, match => match.Value switch {
				Template_DeclarationName => packet.PacketData.DeclarationName,
				Template_Namespace => packet.PacketData.Namespace,
				Template_IdValue => packet.Id.ToString(),
				_ => match.Value
			}));
		});
	}
}
