﻿using System.Text.RegularExpressions;

namespace tModLoader.SourceGenerators;

internal static class Constants
{
	public static Regex MatchBrackets { get; } = new(@"(?:\{[a-zA-Z]*\})", RegexOptions.Compiled);

	// Attribute names
	public const string NetPacketAttributeFullMetadataName = "Terraria.ModLoader.Packets.NetPacketAttribute";
	public const string NetPacketIdOfTAttributeFullName = "Terraria.ModLoader.Packets.NetPacketIdOfAttribute";
	public const string PacketRegistryAttributeFullMetadataName = "Terraria.ModLoader.Packets.PacketRegistryAttribute";

	public const string GloballyEncodedAsAttributeFullName = "Terraria.ModLoader.Packets.GloballyEncodedAsAttribute";
	public const string EncodedAsTAttributeFullName = "Terraria.ModLoader.Packets.EncodedAsAttribute";
	public const string SerializeAttributeFullName = "Terraria.ModLoader.Packets.SerializeAttribute";
	public const string IgnoreAttributeFullName = "Terraria.ModLoader.Packets.IgnoreAttribute";

	// Class names
	public const string ListClassFullName = "System.Collections.Generic.List`1";

	public const string BinaryReaderClassFullName = "System.IO.BinaryReader";
	public const string BinaryWriterClassFullName = "System.IO.BinaryWriter";

	// Struct names
	public const string BitsByteStructFullName = "Terraria.BitsByte";
	public const string HalfStructFullName = "System.Half";
	public const string Vector2StructFullName = "Microsoft.Xna.Framework.Vector2";
	public const string SpanStructFullName = "System.Span`1";
	public const string ReadOnlySpanStructFullName = "System.ReadOnlySpan`1";

	// Interface names
	public const string INetEncoderInterfaceFullName = "Terraria.ModLoader.Packets.INetEncoder";
	public const string INetEncoderTInterfaceFullName = "Terraria.ModLoader.Packets.INetEncoder<T>";
	public const string IDefaultEncoderTInterfaceFullName = "Terraria.ModLoader.Packets.IDefaultEncoder<T>";

	// Properties names
	public const string NetPacketAutoSerializePropertyName = "AutoSerialize";

	// Method names
	public const string NetPacketSetDefaultsMethodName = "SetDefaults";
	public const string NetPacketHandlePacketMethodName = "HandlePacket";
}
