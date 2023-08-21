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
		in AttributeData EncodedAsTData,
		in bool IsUnsafe
	)
	{
		public readonly ITypeSymbol TypeFromEncodedAs =>
			EncodedAsTData.AttributeClass.TypeArguments[0].Interfaces
			.First(x => x.OriginalDefinition.ToDisplayString() == INetEncoderTInterfaceFullName).TypeArguments[0];
	}
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
			encoderTypeName ??= globalEncoders.FirstOrDefault(g => g.Value.Type == propertyTypeName)?.Encoder;

			if (typeSymbol.TypeKind == TypeKind.Array) {
				var elementType = ((IArrayTypeSymbol)typeSymbol).ElementType;
				string builtinEncoder = GetBuiltinTypeEncoder(elementType, globalEncoders);
				if (builtinEncoder == null)
					return null;

				encoderTypeName = $"Terraria.ModLoader.Packets.ArrayEncoder<{elementType.ToDisplayString()}, {builtinEncoder}>";
			}
			else if (typeSymbol.TypeKind == TypeKind.Enum) {
				encoderTypeName = $"Terraria.ModLoader.Packets.EnumEncoder<{typeSymbol.ToDisplayString()}>";
			}
			else {
				switch (typeSymbol.OriginalDefinition.ToDisplayString()) {
					case BitsByteStructFullName:
						encoderTypeName = "Terraria.ModLoader.Packets.ByteEncoder";
						goto End;
					case HalfStructFullName:
						encoderTypeName = "Terraria.ModLoader.Packets.HalfEncoder";
						goto End;
					case Vector2StructFullName:
						encoderTypeName = "Terraria.ModLoader.Packets.Vector2Encoder";
						goto End;
					case ListClassFullName: {
						var elementType = ((INamedTypeSymbol)typeSymbol).TypeArguments[0];
						string builtinEncoder = GetBuiltinTypeEncoder(elementType, globalEncoders);
						if (builtinEncoder == null)
							return null;

						encoderTypeName = $"Terraria.ModLoader.Packets.ListEncoder<{elementType.ToDisplayString()}, {builtinEncoder}>";
						goto End;
					}
					case SpanStructFullName: {
						var elementType = ((INamedTypeSymbol)typeSymbol).TypeArguments[0];
						string builtinEncoder = GetBuiltinTypeEncoder(elementType, globalEncoders);
						if (builtinEncoder == null)
							return null;

						encoderTypeName = $"Terraria.ModLoader.Packets.SpanEncoder<{elementType.ToDisplayString()}, {builtinEncoder}>";
						goto End;
					}
					case ReadOnlySpanStructFullName: {
						var elementType = ((INamedTypeSymbol)typeSymbol).TypeArguments[0];
						string builtinEncoder = GetBuiltinTypeEncoder(elementType, globalEncoders);
						if (builtinEncoder == null)
							return null;

						encoderTypeName = $"Terraria.ModLoader.Packets.ReadOnlySpanEncoder<{elementType.ToDisplayString()}, {builtinEncoder}>";
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
						encoderTypeName = $"Terraria.ModLoader.Packets.ValueTupleEncoder<{typesForEncoder}>";
						goto End;
					}
				}
			}

		End:
			return encoderTypeName ?? GetSpecialTypeEncoder(typeSymbol.SpecialType);
		}

		static string GetSpecialTypeEncoder(SpecialType specialType)
		{
			return specialType switch {
				SpecialType.System_Boolean => "Terraria.ModLoader.Packets.BooleanEncoder",
				SpecialType.System_Char => "Terraria.ModLoader.Packets.CharEncoder",
				SpecialType.System_SByte => "Terraria.ModLoader.Packets.SByteEncoder",
				SpecialType.System_Byte => "Terraria.ModLoader.Packets.ByteEncoder",
				SpecialType.System_Int16 => "Terraria.ModLoader.Packets.ShortEncoder",
				SpecialType.System_UInt16 => "Terraria.ModLoader.Packets.UShortEncoder",
				SpecialType.System_Int32 => "Terraria.ModLoader.Packets.IntEncoder",
				SpecialType.System_UInt32 => "Terraria.ModLoader.Packets.UIntEncoder",
				SpecialType.System_Int64 => "Terraria.ModLoader.Packets.LongEncoder",
				SpecialType.System_UInt64 => "Terraria.ModLoader.Packets.ULongEncoder",
				SpecialType.System_Decimal => "Terraria.ModLoader.Packets.DecimalEncoder",
				SpecialType.System_Single => "Terraria.ModLoader.Packets.SingleEncoder",
				SpecialType.System_Double => "Terraria.ModLoader.Packets.DoubleEncoder",
				SpecialType.System_String => "Terraria.ModLoader.Packets.StringEncoder",
				_ => null
			};
		}

		return serializableProperties.Select(x => {
			var encodeAsAttribute = x.PropertySymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass.OriginalDefinition.ToDisplayString() == EncodedAsTAttributeFullName);

			string encoderTypeName = encodeAsAttribute != null
				? ((ITypeSymbol)encodeAsAttribute.ConstructorArguments[0].Value).ToDisplayString()
				: GetBuiltinTypeEncoder(x.PropertyType, globalEncoders.Cast<GlobalEncoderInfo?>());

			return new EncoderInfo(encoderTypeName, encodeAsAttribute, IsPtr(x.PropertyType));

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
