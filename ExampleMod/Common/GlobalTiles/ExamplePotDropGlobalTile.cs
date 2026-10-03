using ExampleMod.Content.Items;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalTiles
{
	public class ExamplePotDropGlobalTile : GlobalTile
	{
		public override void ModifyPotLoot(PotLoot potLoot) {
			if (potLoot.TileType != TileID.Pots)
				return;

			// Replace the entire reward table for normal pots. Echo pots keep their defaults.
			// Omit Clear() if you only want to add extra rewards.
			potLoot.Clear();
			potLoot.Add(ItemDropRule.Common(ModContent.ItemType<ExampleItem>(), 1, 2, 5));
			potLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 10));

			// Ice pot styles are 4-6. Conditions are checked for the pot being broken.
			if (potLoot.Style >= 4 && potLoot.Style <= 6 && Main.hardMode) {
				potLoot.Add(ItemDropRule.Common(ModContent.ItemType<ExampleSoul>(), 2, 1, 3));
			}

			// Common(item, chanceDenominator, minimumDropped, maximumDropped):
			// ExampleItem always drops 2-5; GoldCoin has a 1/10 chance.
			// ExampleSoul has a 1/2 chance when the condition above is satisfied.
			// Rules run independently, so multiple rewards can drop from one pot.
			// No Load/Unload bookkeeping is needed: each pot receives a fresh loot table.
		}
	}
}
