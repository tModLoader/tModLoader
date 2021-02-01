using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleDataItem : ModItem
	{
		public override string Texture => "ExampleMod/Content/Items/ExampleItem";

		public int timer;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Hot Potato");
			Tooltip.SetDefault("Something magical happens when the timer runs out...");
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			TooltipLine tooltip = new TooltipLine(Mod, "ExampleMod: HotPatato", $"You have {timer / 60f:N1} seconds left!") { overrideColor = Color.Red };
			tooltips.Add(tooltip);
		}

		public override void UpdateInventory(Player player) {
			if (--timer <= 0) {
				player.statLife += 100;
				if (player.statLife > player.statLifeMax2) player.statLife = player.statLifeMax2;
				player.HealEffect(100);
				Item.TurnToAir();
			}
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<ExampleItem>(100);
			(recipe.createItem.ModItem as ExampleDataItem).timer = 300;
			recipe.Register();
		}
	}
}