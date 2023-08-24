using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.Packets;

namespace Terraria.ModLoader.Default;

[NetPacket(typeof(ModLoaderMod))]
public partial struct AccessorySlotInventorySlotNetPacket
{
	private readonly struct ItemEncoder : INetEncoder<Item>
	{
		public void Write(ModPacket packet, Item value) => ItemIO.Send(value, packet);
		public Item Read(BinaryReader reader) => ItemIO.Receive(reader);
	}

	public byte Player { get; set; }
	public sbyte Slot { get; set; }

	[EncodedAs(typeof(ItemEncoder))]
	public Item Item { get; set; }

	public void HandlePacket(int fromWho)
	{
		if (IsClient())
			fromWho = Player;

		var dPlayer = Main.player[fromWho].GetModPlayer<ModAccessorySlotPlayer>();

		ModAccessorySlotPlayer.NetHandler.SetSlot(Slot, Item, dPlayer);

		SendToAllPlayers(ignoreClient: fromWho);
	}

	private static bool IsClient() => Main.netMode == NetmodeID.SinglePlayer;
	private static bool IsServer() => Main.netMode == NetmodeID.Server;

	private readonly bool PreSerialize_Player(BinaryWriter writer, int toClient, int ignoreClient) => IsServer();
	private readonly bool PreDeserialize_Player(BinaryReader reader, int sender) => IsClient();
}

[NetPacket(typeof(ModLoaderMod))]
public partial struct AccessorySlotVisualStateNetPacket
{
	public byte Player { get; set; }
	public sbyte Slot { get; set; }
	public bool HideVisual { get; set; }

	public void HandlePacket(int fromWho)
	{
		if (IsClient())
			fromWho = Player;

		var dPlayer = Main.player[fromWho].GetModPlayer<ModAccessorySlotPlayer>();

		dPlayer.exHideAccessory[Slot] = HideVisual;

		SendToAllPlayers(ignoreClient: fromWho);
	}

	private static bool IsClient() => Main.netMode == NetmodeID.SinglePlayer;
	private static bool IsServer() => Main.netMode == NetmodeID.Server;

	private readonly bool PreSerialize_Player(BinaryWriter writer, int toClient, int ignoreClient) => IsServer();
	private readonly bool PreDeserialize_Player(BinaryReader reader, int sender) => IsClient();
}
