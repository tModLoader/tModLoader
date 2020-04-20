using ExampleMod.Items.Placeable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Tiles
{
	public class ExampleHerb : ModTile
	{
		private static readonly byte pixelsPerStage = 18; //a field for readibilty and to kick out those magic numbers

		public override void SetDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoFail[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);

			TileObjectData.newTile.AnchorValidTiles = new int[]
			{
				TileID.Grass,
				TileID.HallowedGrass,
				TileType<ExampleBlock>()
			};

			TileObjectData.newTile.AnchorAlternateTiles = new int[]
			{
				TileID.ClayPot,
				TileID.PlanterBox
			};

			TileObjectData.addTile(Type);
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
		{
			if (i % 2 == 1)
				spriteEffects = SpriteEffects.FlipHorizontally;
		}

		public override bool Drop(int i, int j)
		{
			Stage stage = GetStage(i, j);

			if (stage == Stage.Grown)
				Item.NewItem(new Vector2(i, j).ToWorldCoordinates(), ItemType<ExampleHerbSeeds>());

			return false;
		}

		public override void RandomUpdate(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			Stage stage = GetStage(i, j);

			if (stage == Stage.Planted || stage == Stage.Growing) {
				tile.frameX += pixelsPerStage;
				NetMessage.SendTileSquare(-1, i, j, 1);
			}
		}

		private Stage GetStage(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j); //Always use Framing.GetTileSafely instead of Main.tile as it prevents any errors caused from other mods
			return (Stage)(tile.frameX / pixelsPerStage);
		}

		private enum Stage : byte
		{
			Planted,
			Growing,
			Grown
		}
	}
}