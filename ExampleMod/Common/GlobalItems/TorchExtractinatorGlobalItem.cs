using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalItems
{
	// These 2 files show off making a new extractinator type. This example converts any torch placing item into any other torch placing item. ModSystem.PostSetupContent is used instead of GlobalItem.SetStaticDefaults to determine which items are torch items because it needs to run after all mods setup their content.
	public class TorchExtractinatorGlobalItem : GlobalItem
	{
		public override void ExtractinatorUse(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack) {
			// If the extractinator type isn't torch, we won't change anything
			if (extractType != ItemID.Torch)
				return;

			// If it is, we set stack to 1 and return a random torch. If the user is using the Chlorophyte Extractinator, we return Ultrabright Torch 10% of the time.
			resultStack = 1;
			if (extractinatorBlockType == TileID.ChlorophyteExtractinator && Main.rand.NextBool(10)) {
				resultType = ItemID.UltrabrightTorch;
				return;
			}

			resultType = Main.rand.Next(TorchExtractinatorModSystem.TorchItems);
		}
	}

	public class TorchExtractinatorModSystem : ModSystem
	{
		internal static List<int> TorchItems;

		public override void PostSetupContent() {
			// Here we iterate through all items and find items that place tiles that are indicated as being torch tiles. We set these items to the extractinator mode of ItemID.Torch to indicate that they all share the torch extractinator result pool.
			ItemID.Sets.ExtractinatorMode[ItemID.Torch] = ItemID.Torch;
			TorchItems = new List<int>();

			for (int i = 0; i < ItemLoader.ItemCount; i++) {
				int createTile = ContentSamples.ItemsByType[i].createTile;
				if (createTile != -1 && TileID.Sets.Torch[createTile] && ItemID.Sets.ExtractinatorMode[i] == -1) {
					ItemID.Sets.ExtractinatorMode[i] = ItemID.Torch;
					TorchItems.Add(i);
				}
			}
		}
	}
}
