using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items
{
	// Making an item like Life Fruit (That goes above 500) involves a lot of code, as there are many things to consider. (An alternate that approaches 500 can simply follow vanilla code, however.):
	//    You can't make player.statLifeMax more than 500 (it won't save), so you'll have to maintain your extra life within your mod.
	//    Within your ModPlayer, you need to save/load a count of usages. You also need to sync the data to other players. 
	internal class ExampleLifeFruit : ModItem
	{
		public override string Texture => "Terraria/Item_" + ItemID.LifeFruit;

		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Permanently increases maximum life by 2\nUp to 10 can be used");
		}

		public override void SetDefaults() {
			item.CloneDefaults(ItemID.LifeFruit);
			item.color = Color.Purple;
		}

		public override bool CanUseItem(Player player) {
			// Any mod that changes statLifeMax to be greater than 500 is broken and needs to fix their code.
			// This check also prevents this item from being used before vanilla health upgrades are maxed out.
			return player.statLifeMax == 500 && player.GetModPlayer<ExamplePlayer>().exampleLifeFruits < ExamplePlayer.maxExampleLifeFruits;
		}

		public override bool UseItem(Player player) {
			// Do not do this: player.statLifeMax += 2;
			player.statLifeMax2 += 2;
			player.statLife += 2;
			if (Main.myPlayer == player.whoAmI) {
				// This spawns the green numbers showing the heal value and informs other clients as well.
				player.HealEffect(2, true);
			}
			// This is very important. This is what makes it permanent.
			player.GetModPlayer<ExamplePlayer>().exampleLifeFruits += 1;
			// This handles the 2 achievements related to using any life increasing item or getting to exactly 500 hp and 200 mp.
			// Ignored since our item is only useable after this achievement is reached
			// AchievementsHelper.HandleSpecialEvent(player, 2);
			return true;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<ExampleItem>());
			recipe.SetResult(this, 11);
			recipe.AddRecipe();
		}
	}
}
