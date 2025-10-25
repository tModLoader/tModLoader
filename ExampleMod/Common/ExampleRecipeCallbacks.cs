using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace ExampleMod.Common
{
	public static class ExampleRecipeCallbacks
	{
		// ConsumeItemCallbacks - These are used to adjust the number of ingredients consumed by recipes, similar to Alchemy Table - See https://github.com/tModLoader/tModLoader/wiki/Intermediate-Recipes#custom-item-consumption
		public static void DontConsumeChain(Recipe recipe, int type, ref int amount, bool isDecrafting) {
			if (type == ItemID.Chain) { // since there is no check for isDecrafting this will not return chains on shimmer either
				amount = 0;
			}
		}
		// Other ConsumeItemCallback methods...

		// OnCraftCallbacks - These are used to run code after a recipe is crafted - See https://github.com/tModLoader/tModLoader/wiki/Intermediate-Recipes#custom-recipe-craft-behavior
		public static void RandomlySpawnFireworks(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack) {
			if (Main.rand.NextBool(3)) {
				int fireworkProjectile = ProjectileID.RocketFireworksBoxRed + Main.rand.Next(4);
				Projectile.NewProjectile(Main.LocalPlayer.GetSource_FromThis(), Main.LocalPlayer.Top, new Microsoft.Xna.Framework.Vector2(0, -Main.rand.NextFloat(2f, 4f)).RotatedByRandom(0.3f), fireworkProjectile, 0, 0, Main.myPlayer);

				Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_FromThis(), ItemID.Confetti, 5);
			}
		}
		// Other OnCraftCallback methods...
	}
}
