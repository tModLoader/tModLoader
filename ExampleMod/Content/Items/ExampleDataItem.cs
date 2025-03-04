using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleDataItem : ModItem
	{
		public override string Texture => "ExampleMod/Content/Items/ExampleItem";

		public int timer;
		public static LocalizedText CountdownText { get; private set; }

		public override void SetStaticDefaults() {
			CountdownText = this.GetLocalization("Countdown");
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			TooltipLine tooltip = new TooltipLine(Mod, "ExampleMod: HotPotato", CountdownText.Format(Math.Round(timer / 60f, 1))) { OverrideColor = Color.Red };
			tooltips.Add(tooltip);
		}

		public override void UpdateInventory(Player player) {
			if (--timer <= 0) {
				player.statLife += 100;
				if (player.statLife > player.statLifeMax2) {
					player.statLife = player.statLifeMax2;
				}
				player.HealEffect(100);
				Item.TurnToAir();
			}
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<ExampleItem>(100);
			((ExampleDataItem)recipe.createItem.ModItem).timer = 300;
			recipe.Register();
		}
	}
}