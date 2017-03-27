using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace ExampleMod.Barrel
{
    public class BarrelItem : ModItem
    {
        public override void SetDefaults()
        {
            item.name = "Barrel";
            item.width = 3;
            item.height = 2;
            item.maxStack = 1;
            AddTooltip("Barrel to collect water or make compost");
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = 1;
            item.consumable = true;
            item.value = Item.sellPrice(0, 0, 0, 50);
            item.createTile = mod.TileType("BarrelTile");
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Wood, 9);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}