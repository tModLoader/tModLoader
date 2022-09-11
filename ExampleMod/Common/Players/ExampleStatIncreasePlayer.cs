using ExampleMod.Content.Items.Consumables;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Common.Players
{
	public class ExampleStatIncreasePlayer : ModPlayer
	{
		public int exampleLifeFruits;
		public int exampleManaCrystals;

		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
			health = StatModifier.Default;
			health.Base = exampleLifeFruits * ExampleLifeFruit.LifePerFruit;
			// Alternatively:  health = StatModifier.Default with { Base = exampleLifeFruits * ExampleLifeFruit.LifePerFruit };
			mana = StatModifier.Default;
			mana.Base = exampleManaCrystals * ExampleManaCrystal.ManaPerCrystal;
			// Alternatively:  mana = StatModifier.Default with { Base = exampleManaCrystals * ExampleManaCrystal.ManaPerCrystal };
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)ExampleMod.MessageType.ExamplePlayerSyncPlayer);
			packet.Write((byte)Player.whoAmI);
			packet.Write((byte)exampleLifeFruits);
			packet.Write((byte)exampleManaCrystals);
			packet.Send(toWho, fromWho);
		}

		// NOTE: The tag instance provided here is always empty by default.
		// Read https://github.com/tModLoader/tModLoader/wiki/Saving-and-loading-using-TagCompound to better understand Saving and Loading data.
		public override void SaveData(TagCompound tag) {
			tag["exampleLifeFruits"] = exampleLifeFruits;
			tag["exampleManaCrystals"] = exampleManaCrystals;
		}

		public override void LoadData(TagCompound tag) {
			exampleLifeFruits = tag.GetInt("exampleLifeFruits");
			exampleManaCrystals = tag.GetInt("exampleManaCrystals");
		}
	}
}
