﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items.Placeable
{
	public class ExampleNonActuatableTile : ModItem
	{
		public override void SetStaticDefaults()
		{
<<<<<<< HEAD:ExampleMod/Items/Placeable/ExampleNonActuatableTile.cs
			DisplayName.SetDefault("Example Non-Actuatable Tile");
			Tooltip.SetDefault("This is block cannot be actuated.");
=======
			DisplayName.SetDefault("Example Non-Actuable Tile");
			Tooltip.SetDefault("This block cannot be actuated.");
>>>>>>> NewCanActuate:ExampleMod/Items/Placeable/ExampleNotActuableTile.cs
		}

		public override void SetDefaults() {
			item.width = 12;
			item.height = 12;
			item.maxStack = 999;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.consumable = true;
			item.createTile = TileType<Tiles.ExampleNonActuatableTile>();
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<ExampleItem>());
			recipe.SetResult(this, 10);
			recipe.AddRecipe();
		}
	}
}
