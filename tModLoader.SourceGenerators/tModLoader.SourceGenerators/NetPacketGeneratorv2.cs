﻿using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using tModLoader.SourceGenerators.Helpers;
using static tModLoader.SourceGenerators.Constants;

namespace tModLoader.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public sealed partial class NetPacketGeneratorv2 : IIncrementalGenerator
{
	private delegate bool SerializationParameterMatch(IParameterSymbol parameterSymbol);
	private delegate bool SerializationMethodMatch(IMethodSymbol methodSymbol);
	private delegate string WriteSerializationParameterMatch(IParameterSymbol parameterSymbol, int index);
	private record struct SerializationMethods(
		in IMethodSymbol PreSerializationSymbol,
		in IMethodSymbol OnSerializationSymbol,
		in IMethodSymbol PostSerializationSymbol
	)
	{
		private readonly INamedTypeSymbol packetSymbol;

		public SerializationMethods(INamedTypeSymbol packetSymbol) : this(null, null, null)
		{
			this.packetSymbol = packetSymbol;
		}

		private readonly IMethodSymbol FindSerializationMethod(string name, SerializationMethodMatch methodMatch, SerializationParameterMatch parameterMatch)
		{
			var enumerable = packetSymbol.GetMembers(name)
				.Where(x => x is IMethodSymbol)
				.Select(x => (IMethodSymbol)x)
				.Where(x => methodMatch(x));

			if (parameterMatch != null) {
				enumerable = enumerable.Where(x => x.Parameters.All(x => parameterMatch(x)));
			}

			return enumerable.FirstOrDefault();
		}

		public void FindPreSerialization(string name, SerializationMethodMatch methodMatch, SerializationParameterMatch match = null)
			=> PreSerializationSymbol = FindSerializationMethod(name, methodMatch, match);

		public void FindOnSerialization(string name, SerializationMethodMatch methodMatch, SerializationParameterMatch match = null)
			=> OnSerializationSymbol = FindSerializationMethod(name, methodMatch, match);

		public void FindPostSerialization(string name, SerializationMethodMatch methodMatch, SerializationParameterMatch match = null)
			=> PostSerializationSymbol = FindSerializationMethod(name, methodMatch, match);

		public readonly void WriteSerializeMethod(IndentedStringBuilder sb, IMethodSymbol methodSymbol, WriteSerializationParameterMatch parameterNames)
		{
			sb.Write(methodSymbol.Name);
			sb.Write('(');
			sb.Write(String.Join(", ", methodSymbol.Parameters.Select((x, i) => parameterNames(x, i))));
			sb.Write(')');
		}
	}
	private record struct SerializationVector(
		in SerializationMethods Serialization,
		in SerializationMethods Deserialization
	);
	private record struct SerializableProperty(
		in ISymbol PropertySymbol,
		in SerializationVector Serialization
	)
	{
		public readonly ITypeSymbol PropertyType => PropertySymbol is IPropertySymbol
				? ((IPropertySymbol)PropertySymbol).Type
				: ((IFieldSymbol)PropertySymbol).Type;
	}

	private record struct SourceSerialization(
		in bool HasPreSerialization, in bool IsPreSerializationBool,
		in bool HasOnSerialization, in bool HasPostSerialization,
		in bool HasPreDeserialization, in bool IsPreDeserializationBool,
		in bool HasOnDeserialization, in bool HasPostDeserialization
	) : IEquatable<SourceSerialization>;
	private record struct SourceProperty(
		in string PropertyName,
		in string PropertyType,
		in SourceSerialization Serialization
	) : IEquatable<SourceProperty>;
	private record struct SourceInfo(
		in string Namespace,
		in string MetadataName,
		in string DeclarationName,
		in bool ImplementsSetDefaults,
		in string ParentModName,

		in (bool Pre, bool IsPreBool, bool On, bool Post) GlobalSerializations,
		in (bool Pre, bool IsPreBool, bool On, bool Post) GlobalDeserializations,
		in EquatableArray<SourceProperty> SerializableProperties,
		in EquatableArray<(string EncoderType, bool IsUnsafe)> Encoders
	) : IEquatable<SourceInfo>;

	private const string SerializeMethodName = "Serialize";
	private const string DeserializeMethodName = "Deserialize";

	private const string Template_Namespace = "{Namespace}";
	private const string Template_DeclarationName = "{DeclarationName}";
	private const string Template_ModName = "{ModName}";
	private const string Template_OptionalSetDefaults = "{OptionalSetDefaults}";
	private const string Template_SerializationImplementation = "{SerializationImplementation}";
	private const string Template_DeserializationImplementation = "{DeserializationImplementation}";

	private const string TemplateCodeText = $@"// <auto-generated/>
#nullable disable
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace {Template_Namespace};

partial struct {Template_DeclarationName} {{
	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Send(int, int)""/>
	[CompilerGenerated]
	public void Send(int toClient = -1, int ignoreClient = -1) {{
		var packet = ModContent.GetInstance<{Template_ModName}>().GetPacket();
		packet.Write(Id);
{Template_SerializationImplementation}
		packet.Send(toClient, ignoreClient);
	}}

	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Receive(BinaryReader, int)""/>
	[CompilerGenerated]
	public void Receive(BinaryReader reader, int sender) {{
		{Template_OptionalSetDefaults}
{Template_DeserializationImplementation}
		if (Main.netMode == NetmodeID.Server) {{
			Send(-1, sender);
		}}
		HandlePacket();
	}}
}}

#nullable restore";

	public void Initialize(IncrementalGeneratorInitializationContext ctx)
	{
		// Commons.AssignDebugger();

		var netPackets = ctx.SyntaxProvider.ForAttributeWithMetadataName(NetPacketAttributeFullMetadataName,
			static (n, _) => n is StructDeclarationSyntax,
			static (ctx, _) => {
				var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
				var netPacketAttribute = ctx.Attributes[0];

				var modType = (ITypeSymbol)netPacketAttribute.ConstructorArguments[0].Value;
				bool autoSerialization = true;
				if (netPacketAttribute.NamedArguments.Any(x => x.Key == NetPacketAutoSerializePropertyName)) {
					autoSerialization = (bool)netPacketAttribute.NamedArguments.First(x => x.Key == NetPacketAutoSerializePropertyName).Value.Value;
				}

				var globalEncodedAsAttributes = RetriveGlobalEncoders(symbol);

				var globalSerializationVector = RetrieveGlobalSerializationMethods(symbol);
				var serializableProperties = RetrieveSerializableProperties(symbol, autoSerialization);
				var encoders = RetriveEncoders(serializableProperties, globalEncodedAsAttributes);

				var sourceProperties = serializableProperties.Select(x => new SourceProperty(
					x.PropertySymbol.Name,
					x.PropertySymbol.ToDisplayString(),
					new SourceSerialization(
						x.Serialization.Serialization.PreSerializationSymbol != null,
						x.Serialization.Serialization.PreSerializationSymbol?.ReturnType?.SpecialType == SpecialType.System_Boolean,
						x.Serialization.Serialization.OnSerializationSymbol != null,
						x.Serialization.Serialization.PostSerializationSymbol != null,

						x.Serialization.Deserialization.PreSerializationSymbol != null,
						x.Serialization.Deserialization.PreSerializationSymbol?.ReturnType?.SpecialType == SpecialType.System_Boolean,
						x.Serialization.Deserialization.OnSerializationSymbol != null,
						x.Serialization.Deserialization.PostSerializationSymbol != null
					)
				)).ToImmutableArray();
				var sourceEncoders = encoders.Select(x => (x.EncoderName, x.IsUnsafe)).ToImmutableArray();

				return new SourceInfo(
					Namespace: symbol.ContainingNamespace.ToString(),
					MetadataName: symbol.MetadataName,
					DeclarationName: symbol.IsRefLikeType ? symbol.Name : $"{symbol.Name} : Terraria.ModLoader.Packets.INetPacket",
					ImplementsSetDefaults: symbol.GetMembers(NetPacketSetDefaultsMethodName).Any(x => x is IMethodSymbol methodSymbol && !methodSymbol.IsGenericMethod && !methodSymbol.Parameters.Any()),
					ParentModName: modType.ToDisplayString(),

					GlobalSerializations: (
						Pre: globalSerializationVector.Serialization.PreSerializationSymbol != null,
						IsPreBool: globalSerializationVector.Serialization.PreSerializationSymbol?.ReturnType?.SpecialType == SpecialType.System_Boolean,
						On: globalSerializationVector.Serialization.OnSerializationSymbol != null,
						Post: globalSerializationVector.Serialization.PostSerializationSymbol != null),
					GlobalDeserializations: (
						Pre: globalSerializationVector.Deserialization.PreSerializationSymbol != null,
						IsPreBool: globalSerializationVector.Deserialization.PreSerializationSymbol?.ReturnType?.SpecialType == SpecialType.System_Boolean,
						On: globalSerializationVector.Deserialization.OnSerializationSymbol != null,
						Post: globalSerializationVector.Deserialization.PostSerializationSymbol != null),

					SerializableProperties: new EquatableArray<SourceProperty>(sourceProperties),
					Encoders: new EquatableArray<(string EncoderType, bool IsUnsafe)>(sourceEncoders)
				);
			});

		ctx.RegisterSourceOutput(netPackets, static (ctx, source) => {
			ctx.AddSource($"{source.Namespace}.{source.MetadataName}.g.cs", MatchBrackets.Replace(TemplateCodeText, match => match.Value switch {
				Template_Namespace => source.Namespace,
				Template_DeclarationName => source.DeclarationName,
				Template_ModName => source.ParentModName,

				Template_OptionalSetDefaults => Create_OptionalSetDefaults(),
				Template_SerializationImplementation => Create_SerializationImplementation(),
				Template_DeserializationImplementation => Create_DeserializationImplementation(),

				_ => match.Value
			}));

			string Create_OptionalSetDefaults()
			{
				if (source.ImplementsSetDefaults)
					return $"{NetPacketSetDefaultsMethodName}();";
				else {
					return $"// {NetPacketSetDefaultsMethodName}();";
				}
			}

			string Create_SerializationImplementation()
			{
				const string ParameterName = "packet";

				var sb = new IndentedStringBuilder();
				sb.Indent += 2;

				if (source.SerializableProperties.IsEmpty)
					return string.Empty;

				sb.WriteLine();

				if (source.GlobalSerializations.Pre) {
					if (source.GlobalSerializations.IsPreBool) {
						sb.Write($"if (PreSerialize({ParameterName})) {{");
						sb.WriteLine();
						sb.Indent++;
					}
					else {
						sb.WriteLine($"PreSerialize({ParameterName})");
					}
				}

				if (source.GlobalSerializations.On) {
					sb.WriteLine($"OnSerialize({ParameterName});");
				}

				int i = 0;
				foreach (var property in source.SerializableProperties.AsSpan()) {
					var encoder = source.Encoders[i++];

					if (property.Serialization.HasPreSerialization) {
						if (property.Serialization.IsPreSerializationBool) {
							sb.Write($"if (PreSerialize_{property.PropertyName}({ParameterName}, {property.PropertyName})) {{");
							sb.WriteLine();
							sb.Indent++;
						}
						else {
							sb.WriteLine($"PreSerialize_{property.PropertyName}({ParameterName}, {property.PropertyName})");
						}
					}

					if (property.Serialization.HasOnSerialization) {
						sb.WriteLine($"OnSerialize_{property.PropertyName}({ParameterName}, {property.PropertyName});");
					}

					if (encoder.IsUnsafe) {
						sb.WriteLine("unsafe {");
						sb.Indent++;
					}

					sb.WriteLine($"var encoder_{property.PropertyName} = default({encoder.EncoderType});");
					sb.WriteLine($"encoder_{property.PropertyName}.Send({ParameterName}, {property.PropertyName});");

					if (property.Serialization.IsPreSerializationBool) {
						sb.Indent--;
						sb.WriteLine('}');
					}

					if (property.Serialization.HasPostSerialization) {
						sb.WriteLine($"PostSerialize_{property.PropertyName}({ParameterName}, {property.PropertyName});");
					}
				}

				if (source.GlobalSerializations.IsPreBool) {
					sb.Indent--;
					sb.WriteLine('}');
				}

				if (source.GlobalSerializations.Post) {
					sb.WriteLine($"PostSerialize({ParameterName});");
				}

				return sb.ToString();
			}

			string Create_DeserializationImplementation()
			{
				const string ParameterName = "reader";

				var sb = new IndentedStringBuilder();
				sb.Indent += 2;

				if (source.SerializableProperties.IsEmpty)
					return string.Empty;

				sb.WriteLine();

				if (source.GlobalSerializations.Pre) {
					if (source.GlobalSerializations.IsPreBool) {
						sb.Write($"if (PreSerialize({ParameterName})) {{");
						sb.WriteLine();
						sb.Indent++;
					}
					else {
						sb.WriteLine($"PreSerialize({ParameterName})");
					}
				}

				if (source.GlobalSerializations.On) {
					sb.WriteLine($"OnSerialize({ParameterName});");
				}

				int i = 0;
				foreach (var property in source.SerializableProperties.AsSpan()) {
					var encoder = source.Encoders[i++];

					if (property.Serialization.HasPreSerialization) {
						if (property.Serialization.IsPreSerializationBool) {
							sb.Write($"if (PreSerialize_{property.PropertyName}({ParameterName}, {property.PropertyName})) {{");
							sb.WriteLine();
							sb.Indent++;
						}
						else {
							sb.WriteLine($"PreSerialize_{property.PropertyName}({ParameterName}, {property.PropertyName})");
						}
					}

					if (property.Serialization.HasOnSerialization) {
						sb.WriteLine($"OnSerialize_{property.PropertyName}({ParameterName}, {property.PropertyName});");
					}

					if (encoder.IsUnsafe) {
						sb.WriteLine("unsafe {");
						sb.Indent++;
					}

					sb.WriteLine($"var encoder_{property.PropertyName} = default({encoder.EncoderType});");
					sb.WriteLine($"{property.PropertyName} = encoder_{property.PropertyName}.Read({ParameterName});");

					if (property.Serialization.IsPreSerializationBool) {
						sb.Indent--;
						sb.WriteLine('}');
					}

					if (property.Serialization.HasPostSerialization) {
						sb.WriteLine($"PostSerialize_{property.PropertyName}({ParameterName}, {property.PropertyName});");
					}
				}

				if (source.GlobalSerializations.IsPreBool) {
					sb.Indent--;
					sb.WriteLine('}');
				}

				if (source.GlobalSerializations.Post) {
					sb.WriteLine($"PostSerialize({ParameterName});");
				}

				return sb.ToString();
			}
		});
	}

	private static SerializationVector RetrieveGlobalSerializationMethods(INamedTypeSymbol symbol)
	{
		static bool MatchMethod(IMethodSymbol methodSymbol)
		{
			return !methodSymbol.IsGenericMethod && methodSymbol.Parameters.Length == 1;
		}

		static bool MatchSerializationParameter(IParameterSymbol parameterSymbol)
		{
			return parameterSymbol.Type.ToDisplayString() is ModPacketClassFullName or BinaryWriterClassFullName;
		}

		static bool MatchDeserializationParameter(IParameterSymbol parameterSymbol)
		{
			return parameterSymbol.Type.ToDisplayString() is BinaryReaderClassFullName;
		}

		var serializationMethods = new SerializationMethods(symbol);
		serializationMethods.FindPreSerialization($"Pre{SerializeMethodName}", MatchMethod, MatchSerializationParameter);
		serializationMethods.FindOnSerialization($"On{SerializeMethodName}", MatchMethod, MatchSerializationParameter);
		serializationMethods.FindPostSerialization($"Post{SerializeMethodName}", MatchMethod, MatchSerializationParameter);

		var deserializationMethods = new SerializationMethods(symbol);
		deserializationMethods.FindPreSerialization($"Pre{DeserializeMethodName}", MatchMethod, MatchDeserializationParameter);
		deserializationMethods.FindOnSerialization($"On{DeserializeMethodName}", MatchMethod, MatchDeserializationParameter);
		deserializationMethods.FindPostSerialization($"Post{DeserializeMethodName}", MatchMethod, MatchDeserializationParameter);

		return new SerializationVector(in serializationMethods, in serializationMethods);
	}

	private static ImmutableArray<SerializableProperty> RetrieveSerializableProperties(INamedTypeSymbol symbol, bool autoSerialization) => symbol.GetMembers()
		.Where(x => {
			bool isValidType =
				x is IFieldSymbol fieldSymbol && !fieldSymbol.IsReadOnly && !fieldSymbol.IsConst
				|| x is IPropertySymbol propertySymbol && (propertySymbol.ReturnsByRef || !propertySymbol.IsWriteOnly && !propertySymbol.IsReadOnly);

			if (!isValidType)
				return false;

			if (autoSerialization) {
				return !x.GetAttributes().Any(x => x.AttributeClass.ToDisplayString() == IgnoreAttributeFullName);
			}
			else {
				return x.GetAttributes().Any(x => x.AttributeClass.ToDisplayString() == SerializeAttributeFullName);
			}
		})
		.Select(x => {
			var typeSymbol = x is IPropertySymbol
				? ((IPropertySymbol)x).Type
				: ((IFieldSymbol)x).Type;

			bool MatchSerializationMethod(IMethodSymbol methodSymbol)
			{
				return !methodSymbol.IsGenericMethod && methodSymbol.Parameters.Length == 2
				&& methodSymbol.Parameters[0].Type.ToDisplayString() is ModPacketClassFullName or BinaryWriterClassFullName
				&& methodSymbol.Parameters[1].Type.ToDisplayString() == typeSymbol.ToDisplayString();
			}

			bool MatchDeserializationMethod(IMethodSymbol methodSymbol)
			{
				return !methodSymbol.IsGenericMethod && methodSymbol.Parameters.Length == 2
				&& methodSymbol.Parameters[0].Type.ToDisplayString() is BinaryReaderClassFullName
				&& methodSymbol.Parameters[1].Type.ToDisplayString() == typeSymbol.ToDisplayString();
			}

			var serialization = new SerializationMethods(symbol);
			serialization.FindPreSerialization($"Pre{SerializeMethodName}_{x.Name}", MatchSerializationMethod);
			serialization.FindOnSerialization($"On{SerializeMethodName}_{x.Name}", MatchSerializationMethod);
			serialization.FindPostSerialization($"Post{SerializeMethodName}_{x.Name}", MatchSerializationMethod);

			var deserialization = new SerializationMethods(symbol);
			deserialization.FindPreSerialization($"Pre{DeserializeMethodName}_{x.Name}", MatchDeserializationMethod);
			deserialization.FindOnSerialization($"On{DeserializeMethodName}_{x.Name}", MatchDeserializationMethod);
			deserialization.FindPostSerialization($"Post{DeserializeMethodName}_{x.Name}", MatchDeserializationMethod);

			return new SerializableProperty(x, new(serialization, deserialization));
		}).ToImmutableArray();
}
