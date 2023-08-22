using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
		in string Type,
		in string Encoder
	);

	private static ImmutableArray<EncoderInfo> RetriveEncoders(ImmutableArray<SerializableProperty> serializableProperties, ImmutableArray<GlobalEncoderInfo> globalEncoders)
	{
		static string GetBuiltinTypeEncoder(ITypeSymbol typeSymbol, IEnumerable<GlobalEncoderInfo?> globalEncoders)
		{
			string encoderTypeName = null;

			string propertyTypeName = typeSymbol.ToDisplayString();
			var globalEncoder = globalEncoders.FirstOrDefault(g => g.Value.Type == propertyTypeName);
			encoderTypeName ??= globalEncoder?.Encoder;
			if (encoderTypeName != null) {
				return $"global::{encoderTypeName}";
			}

			if (typeSymbol.TypeKind == TypeKind.Array) {
				var elementType = ((IArrayTypeSymbol)typeSymbol).ElementType;
				string builtinEncoder = GetBuiltinTypeEncoder(elementType, globalEncoders);
				if (builtinEncoder == null)
					return null;

				encoderTypeName = $"global::Terraria.ModLoader.Packets.ArrayEncoder<{elementType.ToDisplayString()}, {builtinEncoder}>";
			}
			else if (typeSymbol.TypeKind == TypeKind.Enum) {
				encoderTypeName = $"global::Terraria.ModLoader.Packets.EnumEncoder<{typeSymbol.ToDisplayString()}>";
			}
			else {
				switch (typeSymbol.OriginalDefinition.ToDisplayString()) {
					case BitsByteStructFullName:
						encoderTypeName = "global::Terraria.ModLoader.Packets.ByteEncoder";
						goto End;
					case HalfStructFullName:
						encoderTypeName = "global::Terraria.ModLoader.Packets.HalfEncoder";
						goto End;
					case Vector2StructFullName:
						encoderTypeName = "global::Terraria.ModLoader.Packets.Vector2Encoder";
						goto End;
					case ListClassFullName: {
						var elementType = ((INamedTypeSymbol)typeSymbol).TypeArguments[0];
						string builtinEncoder = GetBuiltinTypeEncoder(elementType, globalEncoders);
						if (builtinEncoder == null)
							return null;

						encoderTypeName = $"global::Terraria.ModLoader.Packets.ListEncoder<{elementType.ToDisplayString()}, {builtinEncoder}>";
						goto End;
					}
					case SpanStructFullName: {
						var elementType = ((INamedTypeSymbol)typeSymbol).TypeArguments[0];
						string builtinEncoder = GetBuiltinTypeEncoder(elementType, globalEncoders);
						if (builtinEncoder == null)
							return null;

						encoderTypeName = $"global::Terraria.ModLoader.Packets.SpanEncoder<{elementType.ToDisplayString()}, {builtinEncoder}>";
						goto End;
					}
					case ReadOnlySpanStructFullName: {
						var elementType = ((INamedTypeSymbol)typeSymbol).TypeArguments[0];
						string builtinEncoder = GetBuiltinTypeEncoder(elementType, globalEncoders);
						if (builtinEncoder == null)
							return null;

						encoderTypeName = $"global::Terraria.ModLoader.Packets.ReadOnlySpanEncoder<{elementType.ToDisplayString()}, {builtinEncoder}>";
						goto End;
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
						var typeArgsWithEncoder = typeArgs.Select(x => (Type: x, Encoder: GetBuiltinTypeEncoder(x, globalEncoders)));
						if (typeArgsWithEncoder.Any(x => x.Encoder is null))
							return null;

						string typesForEncoder = string.Join(", ", typeArgsWithEncoder.Select(x => $"{x.Type.ToDisplayString()}, {x.Encoder}"));
						encoderTypeName = $"global::Terraria.ModLoader.Packets.ValueTupleEncoder<{typesForEncoder}>";
						goto End;
					}
				}
			}

		End:
			encoderTypeName ??= GetSpecialTypeEncoder(typeSymbol.SpecialType);

			return encoderTypeName;
		}

		static string GetSpecialTypeEncoder(SpecialType specialType)
		{
			switch (specialType) {
				case SpecialType.System_Boolean:
					return "global::Terraria.ModLoader.Packets.BooleanEncoder";
				case SpecialType.System_Char:
					return "global::Terraria.ModLoader.Packets.CharEncoder";
				case SpecialType.System_SByte:
					return "global::Terraria.ModLoader.Packets.SByteEncoder";
				case SpecialType.System_Byte:
					return "global::Terraria.ModLoader.Packets.ByteEncoder";
				case SpecialType.System_Int16:
					return "global::Terraria.ModLoader.Packets.ShortEncoder";
				case SpecialType.System_UInt16:
					return "global::Terraria.ModLoader.Packets.UShortEncoder";
				case SpecialType.System_Int32:
					return "global::Terraria.ModLoader.Packets.IntEncoder";
				case SpecialType.System_UInt32:
					return "global::Terraria.ModLoader.Packets.UIntEncoder";
				case SpecialType.System_Int64:
					return "global::Terraria.ModLoader.Packets.LongEncoder";
				case SpecialType.System_UInt64:
					return "global::Terraria.ModLoader.Packets.ULongEncoder";
				case SpecialType.System_Single:
					return "global::Terraria.ModLoader.Packets.SingleEncoder";
				case SpecialType.System_Double:
					return "global::Terraria.ModLoader.Packets.DoubleEncoder";
				case SpecialType.System_Decimal:
					return "global::Terraria.ModLoader.Packets.DecimalEncoder";
				case SpecialType.System_String:
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
					if (type.IsDefinition) {
						encoderType = "global::" + encoderType;
					}
				}
				else {
					encoderType = null;
				}
			}
			else {
				encoderTypeName = GetBuiltinTypeEncoder(x.PropertyType, globalEncoders.Cast<GlobalEncoderInfo?>());
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
		.Select(x => new GlobalEncoderInfo(((ITypeSymbol)x.ConstructorArguments[0].Value).ToDisplayString(), ((ITypeSymbol)x.ConstructorArguments[1].Value).ToDisplayString()))
		.ToImmutableArray();
}
