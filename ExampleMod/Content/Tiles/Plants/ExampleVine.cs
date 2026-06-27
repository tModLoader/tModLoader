using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles.Plants
{
	// This is an example of a vine tile. Vine tiles are fairly straightforward, but vines randomly growing and properly converting to other tiles are a bit more complicated. The ExampleVineGlobalTile class below contains the vine growing and converting code and is necessary for a fully working vine.
	public class ExampleVine : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileCut[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileNoFail[Type] = true;

			TileID.Sets.TileCutIgnore.Regrowth[Type] = true;
			TileID.Sets.IsVine[Type] = true;
			TileID.Sets.ReplaceTileBreakDown[Type] = true;
			TileID.Sets.VineThreads[Type] = true;

			AddMapEntry(new Color(160, 160, 160)); // Slightly darker than ExampleBlock

			DustType = DustID.Silver;
			HitSound = SoundID.Grass;
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			// This method is used to make a vine tile draw in the wind. Note that i and j are reversed for this method, this is not a typo.
			Main.instance.TilesRenderer.CrawlToTopOfVineAndAddSpecialPoint(j, i);

			// We must return false here to prevent the normal tile drawing code from drawing the default static tile. Without this a duplicate tile will be drawn.
			return false;
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			offsetY = -2;
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			if (i % 2 == 0) {
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
		}

		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			// Support for https://terraria.wiki.gg/wiki/Guide_to_Plant_Fiber_Cordage. Optional, only some vines do this.
			if (Main.rand.NextBool(2) && WorldGen.GetPlayerForTile(i, j).cordage) {
				yield return new Item(ItemID.VineRope);
			}
		}
	}

	// This class handles spawning and growing ExampleVine (RandomUpdate). Vines can either grow from the tip of an existing vine or spawn from the tile it grows from.
	// Because this behavior needs to act on both ExampleVine and ExampleBlock tiles, we put this logic in a GlobalTile rather than in both ModTile classes.
	// This class also handle transforming vines to ExampleVine if the anchor tile changes (TileFrame).
	public class ExampleVineGlobalTile : GlobalTile
	{
		private int ExampleVine;
		private int ExampleBlock; // TODO: Replace with ExampleGrass eventually.

		public override void SetStaticDefaults() {
			// Caching these tile type values to make the code more readable
			ExampleVine = ModContent.TileType<ExampleVine>();
			ExampleBlock = ModContent.TileType<ExampleBlock>();
		}

		// Random growth behavior:
		public override void RandomUpdate(int i, int j, int type) {
			if (j >= Main.worldSurface - 1) {
				return; // ExampleVine only grows above ground
			}

			Tile tile = Main.tile[i, j];
			if (!tile.HasUnactuatedTile) {
				return; // Don't grow on actuated tiles.
			}

			// Vine tiles usually grow on themselves (from the tip) or on any tile they spawn from (grass tiles usually). GrowMoreVines checks that the nearby area isn't already full of vines.
			if ((tile.TileType == ExampleVine || tile.TileType == ExampleBlock) && WorldGen.GrowMoreVines(i, j)) {
				int growChance = 70;
				if (tile.TileType == ExampleVine) {
					growChance = 7; // 10 times more likely to extend an existing vine than start a new vine
				}

				int below = j + 1;
				Tile tileBelow = Main.tile[i, below];
				if (WorldGen.genRand.NextBool(growChance) && !tileBelow.HasTile && tileBelow.LiquidType != LiquidID.Lava) {
					// We check that the vine can grow longer and is not already broken.
					bool vineIsHangingOffValidTile = false;
					for (int above = j; above > j - 10; above--) {
						Tile tileAbove = Main.tile[i, above];
						if (tileAbove.BottomSlope) {
							return;
						}

						if (tileAbove.HasTile && tileAbove.TileType == ExampleBlock && !tileAbove.BottomSlope) {
							vineIsHangingOffValidTile = true;
							break;
						}
					}

					if (vineIsHangingOffValidTile) {
						// If all the checks succeed, place the tile, copy paint from the tile we grew from, and sync the tile change.
						tileBelow.TileType = (ushort)ExampleVine;
						tileBelow.HasTile = true;
						tileBelow.CopyPaintAndCoating(tile);
						WorldGen.SquareTileFrame(i, below);
						if (Main.netMode == NetmodeID.Server) {
							NetMessage.SendTileSquare(-1, i, below);
						}
					}
				}
			}
		}

		// Transforming vines to ExampleVine if necessary behavior
		public override bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak) {
			// This code handles transforming any vine to ExampleVine if the anchored tile happens to change to ExampleBlock. This can happen with spreading grass tiles or Clentaminator solutions. Without this code the vine would just break in those situations.
			if (!TileID.Sets.IsVine[type]) {
				return true;
			}

			Tile tile = Main.tile[i, j];
			Tile tileAbove = Main.tile[i, j - 1];

			// We determine the tile type of the tile above this tile. If the tile doesn't exist, is actuated, or has a slopped bottom, the vine will be destroyed (-1).
			int aboveTileType = tileAbove.HasUnactuatedTile && !tileAbove.BottomSlope ? tileAbove.TileType : -1;

			// If this tile isn't the same as the one above, we need to verify that the above tile is valid.
			if (type != aboveTileType) {
				// If the above tile is a valid ExampleVine anchor, but this tile isn't ExampleVine, we change this tile into ExampleVine.
				if ((aboveTileType == ExampleBlock || aboveTileType == ExampleVine) && type != ExampleVine) {
					tile.TileType = (ushort)ExampleVine;
					WorldGen.SquareTileFrame(i, j);
					return true;
				}

				// Finally, we need to handle the case where there is not longer a valid placement for ExampleVine.
				// Due to the ordering of hooks with respect to vanilla code, it is not easy to do this in a mod-compatible manner directly. Vanilla vine code or vine code from other mods might convert the vine to a new tile type, but we can't know that here.
				// If the anchor tile is invalid, we kill the tile, otherwise we change the vine tile to TileID.Vines and let the vanilla code that will run after this handle the remaining logic.
				if (type == ExampleVine && aboveTileType != ExampleBlock) {
					if (aboveTileType == -1) {
						WorldGen.KillTile(i, j);
					}
					else {
						tile.TileType = TileID.Vines;
					}
				}
			}

			return true;
		}
	}

	/* With growing or spreading tiles, it can be time consuming to wait for tiles to grow naturally to test their behavior. Debug code like this can help with testing, just be sure to remove it when publishing your mod.
	public class TestVinesSystem : ModSystem
	{
		public override void PostUpdateWorld() {
			if (Main.keyState.IsKeyDown(Keys.D3) && !Main.oldKeyState.IsKeyDown(Keys.D3)) {
				// Spawn vines at the cursor location.
				new ActionVines(3, 8, ModContent.TileType<ExampleVine>()).Apply(new Point(Player.tileTargetX, Player.tileTargetY), Player.tileTargetX, Player.tileTargetY);

				WorldGen.RangeFrame(Player.tileTargetX - 1, Player.tileTargetY - 1, Player.tileTargetX + 1, Player.tileTargetY + 10);
			}
		}
	}
	*/
}
