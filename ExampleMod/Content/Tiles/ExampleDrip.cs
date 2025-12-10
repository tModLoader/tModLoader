using ExampleMod.Content.Liquids;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles
{
	//The tile that spawns the liquid's droplet gore
	//Adapted from vanilla's droplet tiles
	public class ExampleDrip : ModTile
	{
		public override string Texture => $"Terraria/Images/Tiles_{TileID.WaterDrip}";

		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.BreakableWhenPlacing[Type] = true;
			AddMapEntry(new Color(200, 200, 200), CreateMapEntryName());
			DustType = 0;
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			EmitLiquidDrops(i, j, Main.tile[i, j], 180);
			return true;
		}

		private void EmitLiquidDrops(int i, int j, Tile tileCache, int dripChance) {
			if (tileCache.LiquidAmount != 0 || !Main.rand.NextBool(dripChance * 2)) {
				return;
			}
			int type = ModContent.GoreType<ExampleLiquidDroplet>(); //the ID of the droplet this tile drips
			Rectangle positionRect = new(i * 16, j * 16, 16, 16);
			positionRect.X -= 34;
			positionRect.Width += 68;
			positionRect.Y -= 100;
			positionRect.Height = 400;
			for (int k = 0; k < Main.maxGore; k++) {
				if (Main.gore[k].active && GoreID.Sets.LiquidDroplet[Main.gore[k].type]) {
					Rectangle gorePosRect = new((int)Main.gore[k].position.X, (int)Main.gore[k].position.Y, 16, 16);
					if (positionRect.Intersects(gorePosRect)) {
						return;
					}
				}
			}
			Vector2 position = new Vector2(i * 16, j * 16);
			int goreInd = Gore.NewGore(new EntitySource_TileUpdate(i, j), position, default, type);
			Gore gore = Main.gore[goreInd];
			gore.velocity *= 0f;
		}

		public override bool CanPlace(int i, int j) {
			if (!Main.tile[i, j - 1].BottomSlope) {
				int x = Player.tileTargetX;
				int y = Player.tileTargetY - 1;
				if (Main.tile[x, y].HasUnactuatedTile && Main.tileSolid[Main.tile[x, y].TileType] && !Main.tileSolidTop[Main.tile[x, y].TileType]) {
					return true;
				}
			}
			return false;
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile tile = Main.tile[i, j - 1];
			if (!tile.HasTile || tile.BottomSlope || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]) {
				WorldGen.KillTile(i, j);
			}
			return false;
		}

		public override bool CanDrop(int i, int j) {
			return false;
		}

		public override bool CreateDust(int i, int j, ref int type) {
			return false;
		}
	}
}
