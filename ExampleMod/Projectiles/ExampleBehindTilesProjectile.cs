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
		public override string Texture
		{
			get
			{
				return "Terraria/Projectile_3";
			}
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ghost Shuriken");
		}

		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.Shuriken);
			aiType = ProjectileID.Shuriken;
			projectile.hide = true; // Prevents projectile from being drawn normally. Use in conjunction with DrawBehind.
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
			projectile.timeLeft = 60;
		}

		public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
		{
			// Add this projectile to the list of projectiles that will be drawn BEFORE tiles and NPC are drawn. This makes the projectile appear to be BEHIND the tiles and NPC.
			drawCacheProjsBehindNPCsAndTiles.Add(index);
		}
	}
	// This .cs file has 2 classes in it, which is totally fine. (What is important is that namespace+classname is unique. Remember that autoloaded textures follow the namespace+classname convention as well.)
	// This is an approach you can take to fit your organization style.
	public class ExampleBehindTilesProjectileItem : ModItem
	{
		// Use this to use Vanilla textures. The number corresponds to the ItemID of the vanilla item.
		public override string Texture {
			get {
				return "Terraria/Item_42";
			}
		}
		
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ghost Shuriken");
		}

		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.Shuriken);
			item.shoot = mod.ProjectileType("ExampleBehindTilesProjectile");
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
