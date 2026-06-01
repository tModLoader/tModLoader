using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles
{
	// This example demonstrates some advanced tile framing and placement logic necessary for tiles that
	// orient depending on the tile they are anchored to, such as Exposed Gems (the placeable gems) and Crystal Shard.
	// This is a clone of the vanilla ExposedGems tile and contains some gem-specific code.
	// The minimal code necessary for any "multi-directional tile" can be found in CanPlace and TileFrame.
	public class ExampleExposedGem : ModTile
	{
		const int TileHeight = 18;
		const int RandomStyleCount = 3;
		// How much height is between each orientation, comprising the height of 3 random placement styles. TileFrameY values lower than this are pointing up.
		const int StyleHeight = TileHeight * RandomStyleCount;

		Color gemColor = new Color(255, 97, 211);

		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			// We don't use Main.tileShine in this example, see EmitParticles below.
			//Main.tileShine[Type] = 500;
			Main.tileShine2[Type] = true;
			Main.tileObsidianKill[Type] = true;
			Main.tileSpelunker[Type] = true;

			// This example does not use a TileObjectData to implement the placement orientations since it serves as an exact copy of the vanilla tile, but it could certainly be changed to use that approach. This would give the tile a placement preview as well.

			DustType = DustID.GemDiamond; // Rather than make a new dust, reuse and color an existing dust, see CreateDust below.

			AddMapEntry(gemColor, CreateMapEntryName());
		}

		// CanPlace checks for a valid placement location. The conditions for placing this type of tile are more strict than normal terrain tiles.
		public override bool CanPlace(int i, int j) {
			// This code checks that at least one cardinal neighbor is a solid tile. It specifically also checks that the left and right aren't closed doors, since this tile would break when the door opens and that wouldn't be a good user experience.
			if (WorldGen.SolidTile(i - 1, j, noDoors: true) || WorldGen.SolidTile(i + 1, j, noDoors: true) || WorldGen.SolidTile(i, j - 1) || WorldGen.SolidTile(i, j + 1)) {
				return true;
			}

			return false;
		}

		// TileFrame handles orienting the tile based on nearby solid tiles whenever a nearby tile is mined or this tile is placed.
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			// Check for solid tiles in each cardinal direction.
			Tile tile = Main.tile[i, j];
			Tile above = Main.tile[i, j - 1];
			Tile below = Main.tile[i, j + 1];
			Tile left = Main.tile[i - 1, j];
			Tile right = Main.tile[i + 1, j];
			int belowType = -1;
			int aboveType = -1;
			int leftType = -1;
			int rightType = -1;
			if (above != null && above.HasUnactuatedTile && !above.BottomSlope) {
				aboveType = above.TileType;
			}
			if (below != null && below.HasUnactuatedTile && !below.IsHalfBlock && !below.TopSlope) {
				belowType = below.TileType;
			}
			if (left != null && left.HasUnactuatedTile && !left.IsHalfBlock && !left.RightSlope) {
				leftType = left.TileType;
			}
			if (right != null && right.HasUnactuatedTile && !right.IsHalfBlock && !right.LeftSlope) {
				rightType = right.TileType;
			}
			if (TileLoader.IsClosedDoor(leftType)) {
				leftType = -1;
			}
			if (TileLoader.IsClosedDoor(rightType)) {
				rightType = -1;
			}

			// Here we change the tile frame values to orient the tile towards a nearby solid tile. A random placement style is also calculated.
			// The TileFrameY checks ensure that we don't randomize the random placement style if already oriented correctly.
			short randomStyleOffset = (short)(WorldGen.genRand.Next(RandomStyleCount) * TileHeight);
			if (belowType >= 0 && Main.tileSolid[belowType] && !Main.tileSolidTop[belowType]) {
				if (tile.TileFrameY < 0 || tile.TileFrameY >= StyleHeight) {
					tile.TileFrameY = randomStyleOffset;
				}
			}
			else if (leftType >= 0 && Main.tileSolid[leftType] && !Main.tileSolidTop[leftType]) {
				if (tile.TileFrameY < StyleHeight * 2 || tile.TileFrameY >= StyleHeight * 3) {
					tile.TileFrameY = (short)(StyleHeight * 2 + randomStyleOffset);
				}
			}
			else if (rightType >= 0 && Main.tileSolid[rightType] && !Main.tileSolidTop[rightType]) {
				if (tile.TileFrameY < StyleHeight * 3 || tile.TileFrameY >= StyleHeight * 4) {
					tile.TileFrameY = (short)(StyleHeight * 3 + randomStyleOffset);
				}
			}
			else if (aboveType >= 0 && Main.tileSolid[aboveType] && !Main.tileSolidTop[aboveType]) {
				if (tile.TileFrameY < StyleHeight || tile.TileFrameY >= StyleHeight * 2) {
					tile.TileFrameY = (short)(StyleHeight + randomStyleOffset);
				}
			}
			else {
				// If there are no solid tiles in any direction, the tile is killed.
				WorldGen.KillTile(i, j);
			}

			return false; // Since we handled tile framing, we return false.
		}

		// When placed, randomize between the 3 random placement styles.
		public override void PlaceInWorld(int i, int j, Item item) {
			// We only need to randomize for the upward facing styles since TileFrame handles the others before this method is called.
			if (Main.tile[i, j].TileFrameY < StyleHeight) {
				Main.tile[i, j].TileFrameY = (short)(WorldGen.genRand.Next(RandomStyleCount) * TileHeight);
			}
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			// When facing up, move the gem a little lower (sink into the tile below) to make it look better.
			if (tileFrameY < StyleHeight) {
				offsetY = 2;
			}
		}

		// We use EmitParticles to spawn dust randomly.
		// We could use Main.tileShine[Type] = 500; to do this and match the vanilla spawn rate, but vanilla exposed gems have custom Main.tileShine dust colors. Assigning custom colors for Main.tileShine is not supported. This code shows how to replicate that effect without using Main.tileShine, which would spawn completely white dust otherwise.
		public override void EmitParticles(int i, int j, Tile tile, short tileFrameX, short tileFrameY, Color tileLight, bool visible) {
			if (!visible) {
				return;
			}

			if (tileLight.R <= 20 && tileLight.B <= 20 && tileLight.G <= 20) {
				return;
			}

			int lightValue = tileLight.R;
			if (tileLight.G > lightValue) {
				lightValue = tileLight.G;
			}

			if (tileLight.B > lightValue) {
				lightValue = tileLight.B;
			}

			lightValue /= 30;

			const int ParticleRate = 500;
			if (Main.rand.Next(ParticleRate) >= lightValue) {
				return;
			}

			Color dustColor = gemColor;
			int dust = Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, DustID.TintableDustLighted, 0f, 0f, 254, dustColor, 0.5f);
			Main.dust[dust].velocity *= 0f;
		}

		// We use CreateDust to manually spawn the hit dust (DustType assigned in SetStaticDefaults) to customize how it looks.
		public override bool CreateDust(int i, int j, ref int type) {
			int dust = Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, type, 0f, 0f, 75, gemColor, 0.75f);
			Main.dust[dust].noLight = true;

			return false;
		}
	}
}
