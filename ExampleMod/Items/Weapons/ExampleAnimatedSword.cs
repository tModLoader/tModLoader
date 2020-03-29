using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.DataStructures;

namespace ExampleMod.Items.Weapons
{
	public class ExampleAnimatedSword : ModItem
	{
		//PLEASE DO NOT JUST COPY/PASTE THIS CLASS AND EXPECT YOUR SWORD TO ANIMATE WHEN SWUNG. In order for that to occur, you need to insert a custom PlayerLayer. See the ModifyDrawLayers method in ExamplePlayer.
		public override void SetStaticDefaults() 
		{
			Tooltip.SetDefault("This is a basic modded sword.");
            Main.RegisterItemAnimation(item.type, new DrawAnimationVertical(5, 7)); //adds this item's animation data. We have 7 frames and want the frame to switch every 5 ingame ticks.
        }

		public override void SetDefaults() 
		{
			item.damage = 50;
			item.melee = true;
			item.width = 40;
			item.height = 40;
			item.useTime = 60; //slow so we can see our beautiful animation!
			item.useAnimation = 60;
			item.useStyle = 1;
			item.knockBack = 6;
			item.value = 10000;
			item.rare = 2;
            item.noUseGraphic = true; //important so that our entire spritesheet wont draw when the sword is swung!
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;
		}

		public override void AddRecipes() 
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.DirtBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}