using ExampleMod.Content.Items;
using ExampleMod.Content.Tiles;
using ExampleMod.Content.Tiles.Furniture;
using ExampleMod.Content.Walls;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Ammo
{
	public class ExampleSolutionItem : ModItem
	{
		public override string Texture => ExampleMod.AssetPath + "Textures/Items/ExampleSolution";

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Monochromatic Solution");
			Tooltip.SetDefault("Used by the Clentaminator\nSpreads the example");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
		}

		public override void SetDefaults() {
			Item.shoot = ModContent.ProjectileType<ExampleSolutionProjectile>() - ProjectileID.PureSpray;
			Item.ammo = AmmoID.Solution;
			Item.width = 10;
			Item.height = 12;
			Item.value = Item.buyPrice(0, 0, 25);
			Item.rare = ItemRarityID.Orange;
			Item.maxStack = 999;
			Item.consumable = true;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}

	public class ExampleSolutionProjectile : ModProjectile
	{
		public override string Texture => ExampleMod.AssetPath + "Textures/Projectiles/ExampleSolution";

		public ref float Progress => ref Projectile.ai[0];

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Spray");
		}

		public override void SetDefaults() {
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.friendly = true;
			Projectile.alpha = 255;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 2;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override void AI() {
			//Set the dust type to ExampleSolution
			int dustType = ModContent.DustType<Dusts.ExampleSolution>();

			if (Projectile.owner == Main.myPlayer) {
				Convert((int)(Projectile.position.X + (Projectile.width * 0.5f)) / 16, (int)(Projectile.position.Y + (Projectile.height * 0.5f)) / 16, 2);
			}

			if (Projectile.timeLeft > 133) {
				Projectile.timeLeft = 133;
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

				int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, dustType, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100);
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

			Projectile.rotation += 0.3f * Projectile.direction;
		}

		private static void Convert(int i, int j, int size = 4) {
			for (int k = i - size; k <= i + size; k++) {
				for (int l = j - size; l <= j + size; l++) {
					if (WorldGen.InWorld(k, l, 1) && Math.Abs(k - i) + Math.Abs(l - j) < Math.Sqrt((size * size) + (size * size))) {
						int type = Main.tile[k, l].type;
						int wall = Main.tile[k, l].wall;

						//Convert all walls to ExampleWall
						if (wall != 0) {
							Main.tile[k, l].wall = (ushort)ModContent.WallType<ExampleWall>();
							WorldGen.SquareWallFrame(k, l);
							NetMessage.SendTileSquare(-1, k, l, 1);
						}

						//If the tile is stone, convert to ExampleBlock
						if (TileID.Sets.Conversion.Stone[type]) {
							Main.tile[k, l].type = (ushort)ModContent.TileType<ExampleBlock>();
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
							Main.tile[k, l].type = (ushort)ModContent.TileType<ExampleChair>();
							Main.tile[k, l - 1].type = (ushort)ModContent.TileType<ExampleChair>();
							WorldGen.SquareTileFrame(k, l);
							NetMessage.SendTileSquare(-1, k, l, 1);
						}
						//If the tile is a workbench, convert to ExampleWorkBench
						else if (type == TileID.WorkBenches && Main.tile[k - 1, l].type == TileID.WorkBenches) {
							Main.tile[k, l].type = (ushort)ModContent.TileType<ExampleWorkbench>();
							Main.tile[k - 1, l].type = (ushort)ModContent.TileType<ExampleWorkbench>();
							WorldGen.SquareTileFrame(k, l);
							NetMessage.SendTileSquare(-1, k, l, 1);
						}
					}
				}
			}
		}
	}
}