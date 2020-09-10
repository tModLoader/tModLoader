using ExampleMod.Content.Items;
using ExampleMod.Content.Tiles;
using ExampleMod.Content.Tiles.Furniture;
using ExampleMod.Content.Walls;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content
{
	public class ExampleSolutionItem : ModItem
	{
		public override string Texture => ExampleMod.AssetPath + "Textures/Items/ExampleSolution";

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Monochromatic Solution");
			Tooltip.SetDefault("Used by the Clentaminator\nSpreads the example");
		}

		public override void SetDefaults() {
			item.shoot = ProjectileType<ExampleSolutionProjectile>() - ProjectileID.PureSpray;
			item.ammo = AmmoID.Solution;
			item.width = 10;
			item.height = 12;
			item.value = Item.buyPrice(0, 0, 25);
			item.rare = ItemRarityID.Orange;
			item.maxStack = 999;
			item.consumable = true;
		}

		public override void AddRecipes() {
			CreateRecipe(999)
				.AddIngredient<ExampleItem>()
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}

	public class ExampleSolutionProjectile : ModProjectile
	{
		public override string Texture => ExampleMod.AssetPath + "Textures/Projectiles/ExampleSolution";

		public ref float Progress => ref projectile.ai[0];

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Spray");
		}

		public override void SetDefaults() {
			projectile.width = 6;
			projectile.height = 6;
			projectile.friendly = true;
			projectile.alpha = 255;
			projectile.penetrate = -1;
			projectile.extraUpdates = 2;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
		}

		public override void AI() {
			//Set the dust type to ExampleSolution
			int dustType = DustType<Dusts.ExampleSolution>();

			if (projectile.owner == Main.myPlayer) {
				Convert((int)(projectile.position.X + (projectile.width * 0.5f)) / 16, (int)(projectile.position.Y + (projectile.height * 0.5f)) / 16, 2);
			}

			if (projectile.timeLeft > 133) {
				projectile.timeLeft = 133;
			}

			if (Progress > 7f) {
				float dustScale = 1f;

				if (Progress == 8f) {
					dustScale = 0.2f;
				}
				else if (Progress == 9f) {
					dustScale = 0.4f;
				}
				else if (Progress == 10f) {
					dustScale = 0.6f;
				}
				else if (Progress == 11f) {
					dustScale = 0.8f;
				}

				Progress += 1f;

				int dustIndex = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, dustType, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 100);
				Dust dust = Main.dust[dustIndex];
				dust.noGravity = true;
				dust.scale *= 1.75f;
				dust.velocity.X *= 2f;
				dust.velocity.Y *= 2f;
				dust.scale *= dustScale;
			}
			else {
				Progress += 1f;
			}

			projectile.rotation += 0.3f * projectile.direction;
		}

		private static void Convert(int i, int j, int size = 4) {
			for (int k = i - size; k <= i + size; k++) {
				for (int l = j - size; l <= j + size; l++) {
					if (WorldGen.InWorld(k, l, 1) && Math.Abs(k - i) + Math.Abs(l - j) < Math.Sqrt((size * size) + (size * size))) {
						int type = Main.tile[k, l].type;
						int wall = Main.tile[k, l].wall;

						//Convert all walls to ExampleWall
						if (wall != 0) {
							Main.tile[k, l].wall = (ushort)WallType<ExampleWall>();
							WorldGen.SquareWallFrame(k, l);
							NetMessage.SendTileSquare(-1, k, l, 1);
						}

						//If the tile is stone, convert to ExampleBlock
						if (TileID.Sets.Conversion.Stone[type]) {
							Main.tile[k, l].type = (ushort)TileType<ExampleBlock>();
							WorldGen.SquareTileFrame(k, l);
							NetMessage.SendTileSquare(-1, k, l, 1);
						}
						//If the tile is sand, convert to ExampleSand
						// else if (TileID.Sets.Conversion.Sand[type]) {
						// 	Main.tile[k, l].type = (ushort)TileType<ExampleSand>();
						// 	WorldGen.SquareTileFrame(k, l);
						// 	NetMessage.SendTileSquare(-1, k, l, 1);
						// }
						//If the tile is a chair, convert to ExampleChair
						else if (type == TileID.Chairs && Main.tile[k, l - 1].type == TileID.Chairs) {
							Main.tile[k, l].type = (ushort)TileType<ExampleChair>();
							Main.tile[k, l - 1].type = (ushort)TileType<ExampleChair>();
							WorldGen.SquareTileFrame(k, l);
							NetMessage.SendTileSquare(-1, k, l, 1);
						}
						//If the tile is a workbench, convert to ExampleWorkBench
						else if (type == TileID.WorkBenches && Main.tile[k - 1, l].type == TileID.WorkBenches) {
							Main.tile[k, l].type = (ushort)TileType<ExampleWorkbench>();
							Main.tile[k - 1, l].type = (ushort)TileType<ExampleWorkbench>();
							WorldGen.SquareTileFrame(k, l);
							NetMessage.SendTileSquare(-1, k, l, 1);
						}
					}
				}
			}
		}
	}
}