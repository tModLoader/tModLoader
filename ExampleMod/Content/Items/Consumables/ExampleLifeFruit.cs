using ExampleMod.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Consumables
{
	// This file showcases how to create an item that increases the player's maximum health on use.
	// Within your ModPlayer, you need to save/load a count of usages. You also need to sync the data to other players.
	// The overlay used to display the custom life fruit can be found in Common/UI/ResourceDisplay/VanillaLifeOverlay.cs
	internal class ExampleLifeFruit : ModItem
	{
		public static readonly int MaxExampleLifeFruits = 10;
		public static readonly int LifePerFruit = 10;

		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(LifePerFruit, MaxExampleLifeFruits);

		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 10;
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
			if (player.GetModPlayer<ExampleStatIncreasePlayer>().exampleLifeFruits >= MaxExampleLifeFruits) {
				// Returning null will make the item not be consumed
				return null;
			}

			// This method handles permanently increasing the player's max health and displaying the green heal text
			player.UseHealthMaxIncreasingItem(LifePerFruit);

			// This field tracks how many of the example fruit have been consumed
			player.GetModPlayer<ExampleStatIncreasePlayer>().exampleLifeFruits++;

			return true;
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
