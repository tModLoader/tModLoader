using System.Text.RegularExpressions;

namespace tModLoader.SourceGenerators;

internal static class Constants
{
	public static Regex MatchBrackets { get; } = new(@"(?:\{[a-zA-Z]*\})", RegexOptions.Compiled);

	// Attribute names
	public const string INetEncoderTInterfaceFullName = "Terraria.ModLoader.Packets.INetEncoder";
	public const string NetPacketAttributeFullMetadataName = "Terraria.ModLoader.Packets.NetPacketAttribute";
	public const string NetPacketIdOfTAttributeFullName = "Terraria.ModLoader.Packets.NetPacketIdOfAttribute";
	public const string PacketRegistryAttributeFullMetadataName = "Terraria.ModLoader.Packets.PacketRegistryAttribute";

	public const string GloballyEncodedAsAttributeFullName = "Terraria.ModLoader.Packets.GloballyEncodedAsAttribute";
	public const string EncodedAsTAttributeFullName = "Terraria.ModLoader.Packets.EncodedAsAttribute";
	public const string SerializeAttributeFullName = "Terraria.ModLoader.Packets.SerializeAttribute";
	public const string IgnoreAttributeFullName = "Terraria.ModLoader.Packets.IgnoreAttribute";

	// Class names
	public const string ModPacketClassFullName = "Terraria.ModLoader.ModPacket";
	public const string ListClassFullName = "System.Collections.Generic.List<T>";

	public const string BinaryReaderClassFullName = "System.IO.BinaryReader";
	public const string BinaryWriterClassFullName = "System.IO.BinaryWriter";

	// Struct names
	public const string BitsByteStructFullName = "Terraria.BitsByte";
	public const string HalfStructFullName = "System.Half";
	public const string Vector2StructFullName = "Microsoft.Xna.Framework.Vector2";
	public const string SpanStructFullName = "System.Span<T>";
	public const string ReadOnlySpanStructFullName = "System.ReadOnlySpan<T>";

	// Properties names
	public const string NetPacketAutoSerializePropertyName = "AutoSerialize";

	// Method names
	public const string NetPacketSetDefaultsMethodName = "SetDefaults";
}
