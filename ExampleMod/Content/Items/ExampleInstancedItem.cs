using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Content.Items
{
	public class ExampleInstancedItem : ModItem
	{
		public Color[] colors;

		public static LocalizedText EMText { get; private set; }

		public override string Texture => "ExampleMod/Content/Items/ExampleItem";

		public override void SetStaticDefaults() {
			EMText = this.GetLocalization("EM");
		}

		public override void SetDefaults() {
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.Swing;
		}

		public override ModItem Clone(Item item) {
			ExampleInstancedItem clone = (ExampleInstancedItem)base.Clone(item);
			clone.colors = (Color[])colors?.Clone(); // note the ? here is important, colors may be null if spawned from other mods which don't call OnCreate
			return clone;
		}

		public override void OnCreated(ItemCreationContext context) {
			GenerateNewColors();
		}

		private void GenerateNewColors() {
			colors = new Color[5];
			for (int i = 0; i < 5; i++) {
				colors[i] = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.7f);
			}
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (colors == null) // colors may be null if spawned from other mods which don't call OnCreate
				return;

			for (int i = 0; i < colors.Length; i++) {
				TooltipLine tooltipLine = new TooltipLine(Mod, "EM" + i, EMText.Format(i)) { OverrideColor = colors[i] };
				tooltips.Add(tooltipLine);
			}
		}

		public override void UseAnimation(Player player) {
			if (colors == null) {
				GenerateNewColors();
			}
			else {
				// cycle through the colors
				colors = colors.Skip(1).Concat(colors.Take(1)).ToArray();
			}
		}

		// NOTE: The tag instance provided here is always empty by default.
		// Read https://github.com/tModLoader/tModLoader/wiki/Saving-and-loading-using-TagCompound to better understand Saving and Loading data.
		public override void SaveData(TagCompound tag) {
			tag["Colors"] = colors;
		}

		public override void LoadData(TagCompound tag) {
			colors = tag.Get<Color[]>("Colors");
		}

		public override void AddRecipes() => CreateRecipe().AddIngredient<ExampleItem>(10).Register();
	}
}
