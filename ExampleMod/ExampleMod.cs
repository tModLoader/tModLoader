using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod
{
	public class ExampleMod : Mod
	{
		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			ExampleModMessageType msgType = (ExampleModMessageType)reader.ReadByte();
			switch (msgType) {
				// This message syncs ExamplePlayer.exampleLifeFruits
				case ExampleModMessageType.ExamplePlayerSyncPlayer:
					byte playernumber = reader.ReadByte();
					ExamplePlayer examplePlayer = Main.player[playernumber].GetModPlayer<ExamplePlayer>();
					examplePlayer.exampleLifeFruits = reader.ReadInt32();
					// SyncPlayer will be called automatically, so there is no need to forward this data to other clients.
					break;
				default:
					Logger.WarnFormat("ExampleMod: Unknown Message type: {0}", msgType);
					break;
			}
		}
	}

	internal enum ExampleModMessageType : byte
	{
		ExamplePlayerSyncPlayer
	}
}