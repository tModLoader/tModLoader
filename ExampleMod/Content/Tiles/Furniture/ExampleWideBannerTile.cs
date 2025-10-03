using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles.Furniture
{
	/// <summary>
	/// Banner, that unlike most vanilla banners, is wider than one tile.
	/// Showcases TileID.Sets.MultiTileSway and TilesRenderer.AddSpecialPoint to draw the tile swaying in the wind and reacting to player interaction.
	/// </summary>
	public class ExampleWideBannerTile : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.MultiTileSway[Type] = true;

			// This default style defaults to 2x3, despite the name.
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.Origin = Point16.Zero;
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
			TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidBottom | AnchorType.PlanterBox, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.DrawYOffset = -2;

			// This alternate allows for placing the banner on platforms, just like in vanilla.
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.PlatformNonHammered, TileObjectData.newTile.Width, 0);
			TileObjectData.newAlternate.DrawYOffset = -10;
			TileObjectData.addAlternate(0);

			TileObjectData.addTile(Type);
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Main.tile[i, j];

			if (TileObjectData.IsTopLeft(tile)) {
				// Makes this tile sway in the wind and with player interaction when used with TileID.Sets.MultiTileSway
				Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.MultiTileVine);
			}

			// We must return false here to prevent the normal tile drawing code from drawing the default static tile. Without this a duplicate tile will be drawn.
			return false;
		}

		// Unlike ExampleChandelier and ExampleAnimatedTile, this tile does not use AdjustMultiTileVineParameters because the default parameters exhibit the correct physics behavior.
	}
}