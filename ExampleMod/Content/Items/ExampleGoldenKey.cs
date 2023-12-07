using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	// This file showcases how to use the CanOpen hook using a simple bag/item mechanic similar to the Lock Boxes and Golden Keys in the vanilla game.
	// This is the key part and works in tandem with ExampleKeyBag.
	internal class ExampleGoldenKey : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			Item.ToolTip = "Open";
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GoldenKey);
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
