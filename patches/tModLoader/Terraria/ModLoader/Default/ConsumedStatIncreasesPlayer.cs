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
			new ConsumedStatIncreasesPacket {
				Player = (byte)player.whoAmI,
				ConsumedLifeCrystals = (byte)player.ConsumedLifeCrystals,
				ConsumedLifeFruit = (byte)player.ConsumedLifeFruit,
				ConsumedManaCrystals = (byte)player.ConsumedManaCrystals,
			}.Send(toClient, player.whoAmI);
		}
	}
}
