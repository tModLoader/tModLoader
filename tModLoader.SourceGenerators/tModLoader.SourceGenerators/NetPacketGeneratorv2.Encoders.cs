using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using static tModLoader.SourceGenerators.Constants;

namespace tModLoader.SourceGenerators;

partial class NetPacketGeneratorv2
{
	private record struct EncoderInfo(
		in string EncoderName,
		in string TypeFromEncodedAs,
		in bool IsUnsafe
	);
	private record struct GlobalEncoderInfo(
		in string EncodedType,
		in string EncoderName,
		in string EncoderType
	);

	private static ImmutableArray<EncoderInfo> RetrieveEncoders(ImmutableArray<SerializableProperty> serializableProperties, ImmutableArray<GlobalEncoderInfo> globalEncoders)
	{
		static string GetBuiltinTypeEncoder(ITypeSymbol typeSymbol, IEnumerable<GlobalEncoderInfo?> globalEncoders, out StringBuilder sb)
		{
			sb = new();

			string propertyTypeName = typeSymbol.ToDisplayString();
			var globalEncoder = globalEncoders.FirstOrDefault(g => g.Value.EncodedType == propertyTypeName);
			if (globalEncoder != null) {
				sb.Append(globalEncoder.Value.EncoderType);
				return "global::" + globalEncoder.Value.EncoderName;
			}
			else if (typeSymbol.TypeKind == TypeKind.Array) {
				var elementType = ((IArrayTypeSymbol)typeSymbol).ElementType;
				string builtinEncoder = GetBuiltinTypeEncoder(elementType, globalEncoders, out var elementSb);
				if (builtinEncoder == null)
					return null;

				sb.Append(elementSb);
				sb.Append("[]");

				return $"global::Terraria.ModLoader.Packets.ArrayEncoder<{elementType.ToDisplayString()}, {builtinEncoder}>";
			}
			else if (typeSymbol.TypeKind == TypeKind.Enum) {
				sb.Append("global::");
				sb.Append(typeSymbol.ToDisplayString());

				return $"global::Terraria.ModLoader.Packets.EnumEncoder<{typeSymbol.ToDisplayString()}>";
			}
			else {
				switch (typeSymbol.OriginalDefinition.ToDisplayString()) {
					case BitsByteStructFullName:
						return "global::Terraria.ModLoader.Packets.ByteEncoder";
					case HalfStructFullName:
						sb.Append("global::System.Half");
						return "global::Terraria.ModLoader.Packets.HalfEncoder";
					case Vector2StructFullName:
						sb.Append("global::Microsoft.Xna.Framework.Vector2");
						return "global::Terraria.ModLoader.Packets.Vector2Encoder";
					case ListClassFullName: {
						var elementType = ((INamedTypeSymbol)typeSymbol).TypeArguments[0];
						string builtinEncoder = GetBuiltinTypeEncoder(elementType, globalEncoders, out var elementSb);
						if (builtinEncoder == null)
							return null;

						sb.Append("global::System.Collections.Generic.List<");
						sb.Append(elementSb);
						sb.Append('>');

						return $"global::Terraria.ModLoader.Packets.ListEncoder<{elementType.ToDisplayString()}, {builtinEncoder}>";
					}
					case SpanStructFullName: {
						var elementType = ((INamedTypeSymbol)typeSymbol).TypeArguments[0];
						string builtinEncoder = GetBuiltinTypeEncoder(elementType, globalEncoders, out var elementSb);
						if (builtinEncoder == null)
							return null;

						sb.Append("global::System.Span<");
						sb.Append(elementSb);
						sb.Append('>');

						return $"global::Terraria.ModLoader.Packets.SpanEncoder<{elementType.ToDisplayString()}, {builtinEncoder}>";
					}
					case ReadOnlySpanStructFullName: {
						var elementType = ((INamedTypeSymbol)typeSymbol).TypeArguments[0];
						string builtinEncoder = GetBuiltinTypeEncoder(elementType, globalEncoders, out var elementSb);
						if (builtinEncoder == null)
							return null;

						sb.Append("global::System.ReadOnlySpan<");
						sb.Append(elementSb);
						sb.Append('>');

						return $"global::Terraria.ModLoader.Packets.ReadOnlySpanEncoder<{elementType.ToDisplayString()}, {builtinEncoder}>";
					}
				}

				string fullMetadataName = typeSymbol.ContainingNamespace == null
					? typeSymbol.MetadataName
					: $"{typeSymbol.ContainingNamespace}.{typeSymbol.MetadataName}";
				switch (fullMetadataName) {
					case "System.ValueTuple`1":
					case "System.ValueTuple`2":
					case "System.ValueTuple`3":
					case "System.ValueTuple`4":
					case "System.ValueTuple`5":
					case "System.ValueTuple`6":
					case "System.ValueTuple`7":
					case "System.ValueTuple`8": {
						var valueTupleSymbol = (INamedTypeSymbol)typeSymbol;
						var typeArgs = valueTupleSymbol.TypeArguments;
						var typeArgsWithEncoder = typeArgs.Select(x => (Type: x, Encoder: GetBuiltinTypeEncoder(x, globalEncoders, out var sb), sb));
						if (typeArgsWithEncoder.Any(x => x.Encoder is null))
							return null;

						sb.Append('(');
						sb.Append(string.Join(", ", typeArgsWithEncoder.Select(x => x.sb)));
						sb.Append(')');

						string typesForEncoder = string.Join(", ", typeArgsWithEncoder.Select(x => $"{x.Type.ToDisplayString()}, {x.Encoder}"));
						return $"global::Terraria.ModLoader.Packets.ValueTupleEncoder<{typesForEncoder}>";
					}
				}
			}

			return GetSpecialTypeEncoder(typeSymbol.SpecialType, sb);
		}

		static string GetSpecialTypeEncoder(SpecialType specialType, StringBuilder sb)
		{
			switch (specialType) {
				case SpecialType.System_Boolean:
					sb.Append("bool");
					return "global::Terraria.ModLoader.Packets.BooleanEncoder";
				case SpecialType.System_Char:
					sb.Append("char");
					return "global::Terraria.ModLoader.Packets.CharEncoder";
				case SpecialType.System_SByte:
					sb.Append("sbyte");
					return "global::Terraria.ModLoader.Packets.SByteEncoder";
				case SpecialType.System_Byte:
					sb.Append("byte");
					return "global::Terraria.ModLoader.Packets.ByteEncoder";
				case SpecialType.System_Int16:
					sb.Append("short");
					return "global::Terraria.ModLoader.Packets.ShortEncoder";
				case SpecialType.System_UInt16:
					sb.Append("ushort");
					return "global::Terraria.ModLoader.Packets.UShortEncoder";
				case SpecialType.System_Int32:
					sb.Append("int");
					return "global::Terraria.ModLoader.Packets.IntEncoder";
				case SpecialType.System_UInt32:
					sb.Append("uint");
					return "global::Terraria.ModLoader.Packets.UIntEncoder";
				case SpecialType.System_Int64:
					sb.Append("long");
					return "global::Terraria.ModLoader.Packets.LongEncoder";
				case SpecialType.System_UInt64:
					sb.Append("ulong");
					return "global::Terraria.ModLoader.Packets.ULongEncoder";
				case SpecialType.System_Single:
					sb.Append("float");
					return "global::Terraria.ModLoader.Packets.SingleEncoder";
				case SpecialType.System_Double:
					sb.Append("double");
					return "global::Terraria.ModLoader.Packets.DoubleEncoder";
				case SpecialType.System_Decimal:
					sb.Append("decimal");
					return "global::Terraria.ModLoader.Packets.DecimalEncoder";
				case SpecialType.System_String:
					sb.Append("string");
					return "global::Terraria.ModLoader.Packets.StringEncoder";
				default:
					return null;
			}
		}

		return serializableProperties.Select(x => {
			var encodeAsAttribute = x.PropertySymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass.OriginalDefinition.ToDisplayString() == EncodedAsTAttributeFullName);

			string encoderTypeName;
			string encoderType;
			if (encodeAsAttribute != null) {
				var encoder = (ITypeSymbol)encodeAsAttribute.ConstructorArguments[0].Value;
				encoderTypeName = "global::" + encoder.ToDisplayString();

				var type = encoder.Interfaces
					.FirstOrDefault(x => x.OriginalDefinition.ToDisplayString() == INetEncoderTInterfaceFullName)
					?.TypeArguments[0];

				if (type != null) {
					encoderType = type.ToDisplayString();
					if (!type.IsPrimitiveType()) {
						encoderType = "global::" + encoderType;
					}
				}
				else {
					encoderType = null;
				}
			}
			else {
				encoderTypeName = GetBuiltinTypeEncoder(x.PropertyType, globalEncoders.Cast<GlobalEncoderInfo?>(), out var sb);
				encoderType = sb.ToString();
			}

			if (string.IsNullOrEmpty(encoderType)) {
				encoderType = null;
			}

			return new EncoderInfo(encoderTypeName, encoderType, IsPtr(x.PropertyType));

			static bool IsPtr(ITypeSymbol typeSymbol)
			{
				if (typeSymbol is IPointerTypeSymbol or IFunctionPointerTypeSymbol)
					return true;

				if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol) {
					return IsPtr(arrayTypeSymbol.ElementType);
				}
				else if (typeSymbol is INamedTypeSymbol namedTypeSymbol) {
					return namedTypeSymbol.TypeArguments.Any(x => IsPtr(x));
				}

				return false;
			}
		}).Where(x => x.EncoderName != null).ToImmutableArray();
	}

	private static ImmutableArray<GlobalEncoderInfo> RetriveGlobalEncoders(ITypeSymbol symbol) => symbol.GetAttributes()
		.Where(x => x.AttributeClass.ToDisplayString() == GloballyEncodedAsAttributeFullName)
		.Select(x => {
			string serializedType = ((ITypeSymbol)x.ConstructorArguments[0].Value).ToDisplayString();
			var encoder = (ITypeSymbol)x.ConstructorArguments[1].Value;
			var encoderType = encoder.Interfaces.FirstOrDefault(x => x.OriginalDefinition.ToDisplayString() == INetEncoderTInterfaceFullName);
			return new GlobalEncoderInfo(serializedType, encoder.ToDisplayString(), encoderType?.TypeArguments[0]?.ToDisplayString() ?? string.Empty);
		})
		.ToImmutableArray();
}
