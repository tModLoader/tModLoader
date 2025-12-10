using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	// This item showcases dynamically editing item tooltips using ModifyTooltips.
	// Note that for typical tooltips for typical items you would just assign them in the .hjson file and it will automatically be displayed.
	public class ExampleTooltipsItem : ModItem
	{
		public static LocalizedText RemoveMeText { get; private set; }
		public static LocalizedText FaceText { get; private set; }

		public override void SetStaticDefaults() {
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(30, 4));
			ItemID.Sets.AnimatesAsSoul[Type] = true; // Makes the item have an animation while in world (not held.). Use in combination with RegisterItemAnimation

			ItemID.Sets.ItemNoGravity[Type] = true;

			RemoveMeText = this.GetLocalization("RemoveMe");
			FaceText = this.GetLocalization("Face");
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.value = Item.sellPrice(silver: 1);
			Item.rare = ItemRarityID.Blue;
		}

		public override Color? GetAlpha(Color lightColor) {
			return Color.White;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			// Here we add a tooltipline that will later be removed, showcasing how to remove tooltips from an item
			var line = new TooltipLine(Mod, "Verbose:RemoveMe", RemoveMeText.Value);
			tooltips.Add(line);

			line = new TooltipLine(Mod, "Face", FaceText.Value) {
				OverrideColor = new Color(100, 100, 255)
			};
			tooltips.Add(line);

			// Here we give the item name a rainbow effect.
			foreach (TooltipLine line2 in tooltips) {
				if (line2.Mod == "Terraria" && line2.Name == "ItemName") {
					line2.OverrideColor = Main.DiscoColor;
				}
			}

			// Here we will hide all tooltips whose title end with ':RemoveMe'
			// One like that is added at the start of this method
			foreach (var l in tooltips) {
				if (l.Name.EndsWith(":RemoveMe")) {
					l.Hide();
				}
			}

			// Another method of hiding can be done if you want to hide just one line.
			// tooltips.FirstOrDefault(x => x.Mod == "ExampleMod" && x.Name == "Verbose:RemoveMe")?.Hide();
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