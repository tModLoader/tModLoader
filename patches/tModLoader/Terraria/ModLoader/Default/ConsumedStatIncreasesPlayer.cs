using System.IO;
using Terraria.ID;

namespace Terraria.ModLoader.Default;

//Only purpose is for syncing ConsumedLifeCrystals, ConsumedLifeFruit and ConsumedManaCrystals
internal class ConsumedStatIncreasesPlayer : ModPlayer
{
	public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
	{
		NetHandler.SendConsumedState(toWho, Player);
	}

	public override void CopyClientState(ModPlayer targetCopy)
	{
		Player source = Player, target = targetCopy.Player;

		target.ConsumedLifeCrystals = source.ConsumedLifeCrystals;
		target.ConsumedLifeFruit = source.ConsumedLifeFruit;
		target.ConsumedManaCrystals = source.ConsumedManaCrystals;
	}

	public override void SendClientChanges(ModPlayer clientPlayer)
	{
		Player player = Player, client = clientPlayer.Player;

		if (player.ConsumedLifeCrystals != client.ConsumedLifeCrystals || player.ConsumedLifeFruit != client.ConsumedLifeFruit || player.ConsumedManaCrystals != client.ConsumedManaCrystals)
			NetHandler.SendConsumedState(-1, player);
	}

	internal static class NetHandler
	{
		public const byte SyncConsumedProperties = 1;

		public static void SendConsumedState(int toClient, Player player)
		{
			var packet = ModLoaderMod.GetPacket(ModLoaderMod.StatResourcesPacket);

			packet.Write(SyncConsumedProperties);

			if (Main.netMode == NetmodeID.Server)
				packet.Write((byte)player.whoAmI);

			packet.Write((byte)player.ConsumedLifeCrystals);
			packet.Write((byte)player.ConsumedLifeFruit);
			packet.Write((byte)player.ConsumedManaCrystals);

			packet.Send(toClient, player.whoAmI);
		}

		private static void HandleConsumedState(BinaryReader reader, int sender)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				sender = reader.ReadByte();

			Player player = Main.player[sender];

			player.ConsumedLifeCrystals = reader.ReadByte();
			player.ConsumedLifeFruit = reader.ReadByte();
			player.ConsumedManaCrystals = reader.ReadByte();

			if (Main.netMode == NetmodeID.Server)
				SendConsumedState(-1, player);
		}

		public static void HandlePacket(BinaryReader reader, int sender)
		{
			switch (reader.ReadByte()) {
				case SyncConsumedProperties:
					HandleConsumedState(reader, sender);
					break;
			}
		}
	}
}
