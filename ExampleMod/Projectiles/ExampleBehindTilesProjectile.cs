using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Projectiles
{
	public class ExampleBehindTilesProjectile : ModProjectile
	{
		public override bool Autoload(ref string name, ref string texture)
		{
			// Use this to use Vanilla textures. The number corresponds to the ProjectileID of hte vanilla projectile.
			texture = "Terraria/Projectile_3";
			return true;
		}

		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.Shuriken);
			projectile.name = "Ghost Shuriken";
			aiType = ProjectileID.Shuriken;
			projectile.hide = true; // Prevents projectile from being drawn normally. Use in conjunction with DrawBehind.
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
			projectile.timeLeft = 60;
		}

		public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles)
		{
			// Add this projectile to the list of projectiles that will be drawn BEFORE tiles and NPC are drawn. This makes the projectile appear to be BEHIND the tiles and NPC.
			drawCacheProjsBehindNPCsAndTiles.Add(index);
		}
	}
	// This .cs file has 2 classes in it, which is totally fine. (What is important is that namespace+classname is unique. Remember that autoloaded textures follow the namespace+classname convention as well.)
	// This is an approach you can take to fit your organization style.
	public class ExampleBehindTilesProjectileItem : ModItem
	{
		public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
		{
			// Use this to use Vanilla textures. The number corresponds to the ItemID of the vanilla item.
			texture = "Terraria/Item_42";
			return true;
		}

		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.Shuriken);
			item.shoot = mod.ProjectileType("ExampleBehindTilesProjectile");
			item.name = "Ghost Shuriken";
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Shuriken, 10);
			recipe.AddIngredient(null, "ExampleItem", 1);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(this, 10);
			recipe.AddRecipe();
		}
	}
}
