using ExampleMod.Mounts;
using ExampleMod.Tiles;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class MinecartKeys : ModItem
	{
		public override string Texture => "Terraria/Item_" + ItemID.MechanicalWheelPiece; //use a vanilla item texture
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("This is a modded minecart.");
		}

		public override void SetDefaults()
		{
			item.color = Color.Purple; //changing vanilla item color
			item.width = 20;
			item.height = 30;
			item.useTime = 20;
			item.useAnimation = 20;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.value = 30000;
			item.rare = ItemRarityID.Green;
			item.UseSound = SoundID.Item79;
			item.noMelee = true;
			MountID.Sets.Cart[ModContent.MountType<ExampleMinecart>()] = true; //this line makies it automatically equip into aminecart slot
			item.mountType = ModContent.MountType<ExampleMinecart>();
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<ExampleItem>(), 10);
			recipe.AddTile(ModContent.TileType<ExampleWorkbench>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}