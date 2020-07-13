using Terraria.ModLoader;

namespace ExampleMod.Content
{
	public class ExamplePlayer : ModPlayer
	{
		public const int MaxExampleLifeFruits = 10;
	
		public int exampleLifeFruits;

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
			ModPacket packet = mod.GetPacket();
			packet.Write((byte)ExampleModMessageType.ExamplePlayerSyncPlayer);
			packet.Write((byte)player.whoAmI);
			packet.Write(exampleLifeFruits);
			packet.Send(toWho, fromWho);
		}

		public override TagCompound Save() {
			// Read https://github.com/tModLoader/tModLoader/wiki/Saving-and-loading-using-TagCompound to better understand Saving and Loading data.
			return new TagCompound {
				["exampleLifeFruits"] = exampleLifeFruits
			};
		}

		public override void Load(TagCompound tag) {
			exampleLifeFruits = tag.GetInt("exampleLifeFruits");
		}
	}
}