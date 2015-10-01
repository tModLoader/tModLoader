using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Placeable {
public class ExampleBlock : ModItem
{
    public override void SetDefaults()
    {
        item.name = "Example Block";
        item.width = 12;
        item.height = 12;
        item.maxStack = 999;
        AddTooltip("This is a modded block.");
        item.useTurn = true;
        item.autoReuse = true;
        item.useAnimation = 15;
        item.useTime = 10;
        item.useStyle = 1;
        item.consumable = true;
        item.createTile = mod.TileType("ExampleBlock");
    }

    public override void AddRecipes()
    {
        ModRecipe recipe = new ModRecipe(mod);
        recipe.AddIngredient(null, "ExampleItem");
        recipe.SetResult(this, 10);
        recipe.AddRecipe();

        recipe = new ModRecipe(mod);
        recipe.AddIngredient(null, "ExampleWall", 4);
        recipe.SetResult(this);
        recipe.AddTile(null, "ExampleWorkbench");
        recipe.AddRecipe();

        recipe = new ModRecipe(mod);
        recipe.AddIngredient(null, "ExamplePlatform", 2);
        recipe.SetResult(this);
        recipe.AddRecipe();
    }
}}