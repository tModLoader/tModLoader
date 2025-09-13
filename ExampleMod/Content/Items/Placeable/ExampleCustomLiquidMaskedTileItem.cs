using ExampleMod.Content.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable;

// Basic item to place the tile of interest
public sealed class ExampleCustomLiquidMaskedTileItem : ModItem
{
	public override void SetDefaults() {
		Item.DefaultToPlaceableTile(ModContent.TileType<ExampleCustomLiquidMaskedTile>());
	}

	public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient<ExampleBlock>()
			.Register();
	}
}
