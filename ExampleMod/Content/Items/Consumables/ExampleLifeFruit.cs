using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod.Common.Players;

namespace ExampleMod.Content.Items.Consumables
{
	// This file showcases how to create an item that increases the player's maximum health on use.
	// Within your ModPlayer, you need to save/load a count of usages. You also need to sync the data to other players.
	// The overlay used to display the custom life fruit can be found in Common/UI/ResourceDisplay/VanillaLifeOverlay.cs
	internal class ExampleLifeFruit : ModItem
	{
		public const int MaxExampleLifeFruits = 10;
		public const int LifePerFruit = 10;

		public override void SetStaticDefaults() {
			Tooltip.SetDefault($"Permanently increases maximum life by {LifePerFruit}\nUp to {MaxExampleLifeFruits} can be used");

			SacrificeTotal = 10;
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LifeFruit);
		}

		public override bool CanUseItem(Player player) {
			// This check prevents this item from being used before vanilla health upgrades are maxed out.
			return player.ConsumedLifeCrystals == Player.LifeCrystalMax && player.ConsumedLifeFruit == Player.LifeFruitMax;
		}

		public override bool? UseItem(Player player) {
			// Moving the exampleLifeFruits check from CanUseItem to here allows this example fruit to still "be used" like Life Fruit can be
			// when at the max allowed, but it will just play the animation and not affect the player's max life
			bool canConsume = false;
			if (player.GetModPlayer<ExampleStatIncreasePlayer>().exampleLifeFruits < MaxExampleLifeFruits) {
				canConsume = true;

				// This method handles permanently increasing the player's max health and displaying the green heal text
				player.UseHealthMaxIncreasingItem(LifePerFruit);

				// This field tracks how many of the example fruit have been consumed
				player.GetModPlayer<ExampleStatIncreasePlayer>().exampleLifeFruits++;
				// This handles the 2 achievements related to using any life increasing item or reaching the max amount of consumed Life Crystals, Life Fruit and Mana Crystals.
				// Ignored since our item is only useable after this achievement is reached
				// AchievementsHelper.HandleSpecialEvent(player, 2);
				//TODO re-add this when ModAchievement is merged?
			}
			// Returning null will make the item not be consumed
			return canConsume ? true : null;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
