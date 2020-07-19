using ExampleMod.Content.Items;
using ExampleMod.Content.Items.Consumables;
using System.IO;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod
{
	public class ExampleMod : Mod
	{
		public override void AddRecipeGroups() {
			//Creates and registers the new recipe group with the specified name
			RecipeGroup.RegisterGroup(
				"ExampleMod:ExampleItem",
				new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ModContent.ItemType<ExampleItem>())}", ModContent.ItemType<ExampleItem>()));
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			ExampleModMessageType msgType = (ExampleModMessageType)reader.ReadByte();
			switch (msgType) {
				// This message syncs ExamplePlayer.exampleLifeFruits
				case ExampleModMessageType.ExamplePlayerSyncPlayer:
					byte playernumber = reader.ReadByte();
					ExampleLifeFruitPlayer examplePlayer = Main.player[playernumber].GetModPlayer<ExampleLifeFruitPlayer>();
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
		ExamplePlayerSyncPlayer,
		ExampleTeleportToStatue
	}
}