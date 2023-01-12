using ExampleMod.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Consumables
{
	// This file showcases how to create an item that increases the player's maximum mana on use.
	// Within your ModPlayer, you need to save/load a count of usages. You also need to sync the data to other players.
	// The overlay used to display the custom mana crystals can be found in Common/UI/ResourceDisplay/VanillaManaOverlay.cs
	internal class ExampleManaCrystal : ModItem
	{
		public static readonly int MaxExampleManaCrystals = 10;
		public static readonly int ManaPerCrystal = 10;

		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ManaPerCrystal, MaxExampleManaCrystals);

		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 10;
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ManaCrystal);
		}

		public override bool CanUseItem(Player player) {
			// This check prevents this item from being used before vanilla mana upgrades are maxed out.
			return player.ConsumedManaCrystals == Player.ManaCrystalMax;
		}

		public override bool? UseItem(Player player) {
			// Moving the exampleManaCrystals check from CanUseItem to here allows this example crystal to still "be used" like Mana Crystals can be
			// when at the max allowed, but it will just play the animation and not affect the player's max mana
			if (player.GetModPlayer<ExampleStatIncreasePlayer>().exampleManaCrystals >= MaxExampleManaCrystals) {
				// Returning null will make the item not be consumed
				return null;
			}

			// This method handles permanently increasing the player's max mana and displaying the blue mana text
			player.UseManaMaxIncreasingItem(ManaPerCrystal);

			// This field tracks how many of the example crystals have been consumed
			player.GetModPlayer<ExampleStatIncreasePlayer>().exampleManaCrystals++;

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
