using ExampleMod.Content.Items;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalTiles
{
	/// <summary>
	/// Demonstrates modifying the vanilla pot drop rules. Vanilla drops remain enabled unless Clear is called.
	/// </summary>
	public class ExamplePotDropGlobalTile : GlobalTile
	{
		public override void ModifyPotLoot(PotLoot potLoot) {
			if (potLoot.TileType == TileID.Pots) {
				if (potLoot.Style >= 4 && potLoot.Style <= 6) {
					potLoot.Add(ItemDropRule.Common(ModContent.ItemType<ExampleSoul>(), 2, 1, 3));
				}

				// To replace vanilla pot drops completely, use this before adding your own rules:
				// potLoot.Clear();
				// potLoot.Add(ItemDropRule.Common(ModContent.ItemType<ExampleItem>(), 1, 2, 5));
			}
		}
	}
}
