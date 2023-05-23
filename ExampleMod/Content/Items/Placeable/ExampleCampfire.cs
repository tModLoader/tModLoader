﻿using ExampleMod.Content.Dusts;
using ExampleMod.Content.Tiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleCampfire : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.ShimmerCampfire;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.ExampleCampfire>(), 0);
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddRecipeGroup(RecipeGroupID.Wood, 10)
				.AddIngredient<ExampleTorch>(5)
				.Register();
		}
	}
}
