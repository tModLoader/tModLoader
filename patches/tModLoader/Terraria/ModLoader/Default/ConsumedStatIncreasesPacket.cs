using System.IO;
using Terraria.ID;
using Terraria.ModLoader.Packets;

namespace Terraria.ModLoader.Default;

[NetPacket(typeof(ModLoaderMod))]
public partial struct ConsumedStatIncreasesPacket
{
	public byte Player { get; set; }
	public byte ConsumedLifeCrystals { get; set; }
	public byte ConsumedLifeFruit { get; set; }
	public byte ConsumedManaCrystals { get; set; }

	public readonly void HandlePacket()
	{
	}

	public readonly void HandlePacket(int sender)
	{
		if (IsMPClient())
			sender = Player;

		Player player = Main.player[sender];

		player.ConsumedLifeCrystals = ConsumedLifeCrystals;
		player.ConsumedLifeFruit = ConsumedLifeFruit;
		player.ConsumedManaCrystals = ConsumedManaCrystals;
	}

	private static bool IsMPClient() => Main.netMode == NetmodeID.MultiplayerClient;
	private static bool IsServer() => Main.netMode == NetmodeID.Server;

	private readonly bool PreSerialize_Player(BinaryWriter writer, int toClient, int ignoreClient) => IsServer();
	private readonly bool PreDeserialize_Player(BinaryReader reader, int sender) => IsMPClient();
}
