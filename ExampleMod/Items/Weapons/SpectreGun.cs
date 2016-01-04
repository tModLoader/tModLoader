using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Weapons
{
	//ported from my tAPI mod because I'm lazy
	public class SpectreGun : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Spectre Gun";
			item.damage = 53;
			item.ranged = true;
			item.width = 42;
			item.height = 30;
			item.toolTip = "Uses wisps as ammo";
			item.useTime = 35;
			item.useAnimation = 35;
			item.useStyle = 5;
			item.noMelee = true;
			item.knockBack = 4f;
			item.value = Item.sellPrice(0, 10, 0, 0);
			item.rare = 8;
			item.useSound = 43;
			item.autoReuse = true;
			item.shoot = mod.ProjectileType("Wisp");
			item.shootSpeed = 6f;
			item.useAmmo = mod.ProjectileType("Wisp");
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem", 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override Vector2? HoldoutOffset()
		{
			return Vector2.Zero;
		}
	}
}