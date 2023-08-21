using ExampleMod.Common.Packets;
using Terraria.ModLoader.Packets;

namespace ExampleMod.Common;

[PacketRegistry]
public static partial class PacketRegistry
{
	[NetPacketIdOf(typeof(ExampleStatIncreasePlayerPacket))]
	public const byte ExampleStatIncreasePlayerId = 0;
}
