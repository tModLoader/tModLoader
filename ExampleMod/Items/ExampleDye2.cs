using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Graphics.Shaders;

namespace ExampleMod.Items //Make sure you look at ExampleMod.cs for explanation on how this dye works! The dye's code in ExampleMod.cs is located in the Load hook.
{
    public class ExampleDye2 : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Example Dye");
        }

        public override void SetDefaults()
        {
            item.width = 16;
            item.height = 16;
            item.maxStack = 99;
            item.rare = ItemRarityID.Red;
            item.dye = (byte)GameShaders.Armor.GetShaderIdFromItemId(item.type); //This is not optional, it actually makes the dye function.
        }

        public override void AddRecipes()
        {
            ModRecipe modRecipe = new ModRecipe(mod);
            modRecipe.AddIngredient(ItemID.BottledWater);
            modRecipe.SetResult(this);
            modRecipe.AddRecipe();
        }
    }
}