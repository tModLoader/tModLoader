using ExampleMod.Dusts;
using ExampleMod.Projectiles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Tiles
{
	public class ExampleSandBlock : ModTile 
	{
		//Note: ExampleSandBlock requires ExampleSandProjectile to work.
		//This is how the block works:
		//TH block is placed on another solid block. When the block below it is destroyed , the original block gets destroyed and spawns a projectile
		//That projectile spawns dust. When that projectile hits another tile, it will create the sand tile again.

		//ExampleSandBlock (the item) is just used for placing the tile, this isn't needed and can be placed in other ways

		public override void SetDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBrick[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileSand[Type] = true;
			TileID.Sets.TouchDamageSands[Type] = 15;
			TileID.Sets.Falling[Type] = true;
			AddMapEntry(new Color(200, 200, 200));
			dustType = ModContent.DustType<Sparkle>();
			drop = ModContent.ItemType<Items.Placeable.ExampleSandBlock>();
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			if (WorldGen.noTileActions) {
				return true;
			}

			Tile tile = Main.tile[i, j + 1];

			Tile above = Main.tile[i, j - 1];
			Tile below = tile;
			bool canFall = true;

			if (below == null || below.active()) {
				canFall = false;
			}

			if (above.active() && (TileID.Sets.BasicChest[above.type] || TileID.Sets.BasicChestFake[above.type] || above.type == TileID.PalmTree || TileLoader.IsDresser(above.type))) {
				canFall = false;
			}

			if (canFall) {
				int projectileType = ModContent.ProjectileType<ExampleSandProjectile>();
				float positionX = i * 16 + 8;
				float positionY = j * 16 + 8;

				if (Main.netMode == 0) {
					Main.tile[i, j].ClearTile();
					int proj = Projectile.NewProjectile(positionX, positionY, 0f, 0.41f, projectileType, 10, 0f, Main.myPlayer, 0f, 0f);
					Main.projectile[proj].ai[0] = 1f;
					WorldGen.SquareTileFrame(i, j, true);
				}
				else if (Main.netMode == 2) {
					Main.tile[i, j].active(false);
					bool spawnProj = true;

					for (int k = 0; k < 1000; k++) {
						Projectile otherProj = Main.projectile[k];

						if (otherProj.active && otherProj.owner == Main.myPlayer && otherProj.type == projectileType && Math.Abs(otherProj.timeLeft - 3600) < 60 && otherProj.Distance(new Vector2(positionX, positionY)) < 4f) {
							spawnProj = false;
							break;
						}
					}

					if (spawnProj) {
						int proj = Projectile.NewProjectile(positionX, positionY, 0f, 2.5f, projectileType, 10, 0f, Main.myPlayer, 0f, 0f);
						Main.projectile[proj].velocity.Y = 0.5f;
						Main.projectile[proj].position.Y += 2f;
						Main.projectile[proj].netUpdate = true;
					}

					NetMessage.SendTileSquare(-1, i, j, 1);
					WorldGen.SquareTileFrame(i, j, true);
				}
				return false;
			}
			return true;
		}
	}
}