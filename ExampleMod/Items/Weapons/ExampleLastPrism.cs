using ExampleMod.Projectiles;
using ExampleMod.Tiles;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Weapons
{
	public class ExampleLastPrism : ModItem
	{
		// You can use a vanilla texture for your item by using the format: "Terraria/Item_<Item ID>".
		public override string Texture => "Terraria/Item_" + ItemID.LastPrism;
		public static Color OverrideColor = new Color(122, 173, 255);
		
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Example Last Prism");
			Tooltip.SetDefault(@"A slightly different laser-firing Prism
Ignores NPC immunity frames and fires 10 beams at once instead of 6.");
		}

		public override void SetDefaults()
		{
			// Start by using CloneDefaults to clone all the basic item properties from the vanilla Last Prism.
			// For example, this copies sprite size, use style, sell price, and the item being a magic weapon.
			item.CloneDefaults(ItemID.LastPrism);
			item.mana = 4;
			item.damage = 42;
			item.shoot = ModContent.ProjectileType<ExampleLastPrismHoldout>();
			item.shootSpeed = 30f;

			// Change the item's draw color so that it is visually distinct from the vanilla Last Prism.
			item.color = OverrideColor;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<ExampleItem>(), 10);
			recipe.AddTile(ModContent.TileType<ExampleWorkbench>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		// Because this weapon fires a holdout projectile, it needs to block usage if its projectile already exists.
		public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ModContent.ProjectileType<ExampleLastPrismHoldout>()] <= 0;
	}
}
