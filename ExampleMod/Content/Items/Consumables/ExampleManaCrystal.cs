using ExampleMod.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Consumables
{
	// This file showcases how to create an item that increases the player's maximum mana on use.
	// Within your ModPlayer, you need to save/load a count of usages. You also need to sync the data to other players.
	// The overlay used to display the custom mana crystals can be found in Common/UI/ResourceDisplay/VanillaManaOverlay.cs
	// The code reponsible for tracking and modifying how many extra mana stars/bars are displayed can be found in Common/Systems/ExampleStatIncreaseSystem.cs
	internal class ExampleManaCrystal : ModItem
	{
		public const int MaxExampleManaCrystals = 10;
		public const int ManaPerCrystal = 10;

		public override void SetStaticDefaults() {
			Tooltip.SetDefault($"Permanently increases maximum mana by {ManaPerCrystal}\nUp to {MaxExampleManaCrystals} can be used");

			SacrificeTotal = 10;
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ManaCrystal);
		}

		public override bool CanUseItem(Player player) {
			// This check prevents this item from being used before vanilla mana upgrades are maxed out.
			return player.ConsumedManaCrystals == Player.ManaCrystalMax;
		}

		public override bool? UseItem(Player player) {
			// Moving the exampleLifeFruits check from CanUseItem to here allows this example fruit to still "be used" like Mana Crystals can be
			// when at the max allowed, but it will just play the animation and not affect the player's max life
			bool canConsume = false;
			if (player.GetModPlayer<ExampleStatIncreasePlayer>().exampleManaCrystals < MaxExampleManaCrystals) {
				canConsume = true;

				// This method handles permanently increasing the player's max health and displaying the green heal text
				player.UseManaMaxIncreasingItem(ManaPerCrystal);

				// This field tracks how many of the example crystals have been consumed
				player.GetModPlayer<ExampleStatIncreasePlayer>().exampleManaCrystals++;
				// This handles the 2 achievements related to using any mana increasing item or reaching the max amount consumed of Life Crystals, Life Fruit and Mana Crystals.
				// Ignored since our item is only useable after this achievement is reached
				// AchievementsHelper.HandleSpecialEvent(player, 1);
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
