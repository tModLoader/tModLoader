using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Content.Items
{
	public class ExampleInstancedItem : ModItem
	{
		public override string Texture => "ExampleMod/Content/Items/ExampleItem";

		public override ModItem Clone(Item item) {
			ExampleInstancedItem clone = (ExampleInstancedItem)base.Clone(item);
			clone.colors = (Color[])colors.Clone();
			return clone;
		}

		public Color[] colors;

		public override void OnCreate(ItemCreationContext context) {
			colors = new Color[5];
			for (int i = 0; i < 5; i++) {
				colors[i] = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.7f);
			}
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = 0; i < colors.Length; i++) {
				TooltipLine tooltipLine = new TooltipLine(Mod, "EM" + i, "Example " + i) { overrideColor = colors[i] };
				tooltips.Add(tooltipLine);
			}
		}

		public override TagCompound Save() =>
			new TagCompound { ["Colors"] = colors.ToList() };

		public override void Load(TagCompound tag) {
			colors = tag.GetList<Color>("Colors").ToArray();
		}

		public override void AddRecipes() => CreateRecipe().AddIngredient<ExampleItem>(10).Register();
	}
}