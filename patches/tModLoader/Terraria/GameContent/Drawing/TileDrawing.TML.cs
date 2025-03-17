using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.ModLoader;

namespace Terraria.GameContent.Drawing;

public partial class TileDrawing
{
	/// <summary>
	/// The wind grid used to exert wind effects on tiles.
	/// </summary>
	public WindGrid Wind => _windGrid;

	/// <summary>
	/// Checks if a tile at the given coordinates counts towards tile coloring from the Dangersense buff.
	/// <br/>Vanilla only uses Main.LocalPlayer for <paramref name="player"/>
	/// </summary>
	public static bool IsTileDangerous(int tileX, int tileY, Player player)
	{
		Tile tile = Main.tile[tileX, tileY];
		return IsTileDangerous(tileX, tileY, player, tile, tile.type);
	}

	private void DrawCustom(bool solidLayer)
	{
		if (solidLayer) {
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
		}

		int index = (int)(solidLayer ? TileCounterType.CustomSolid : TileCounterType.CustomNonSolid);
		int specialCount = _specialsCount[index];
		for (int i = 0; i < specialCount; i++) {
			Point p = _specialPositions[index][i];
			Tile tile = Main.tile[p.X, p.Y];
			TileLoader.SpecialDraw(tile.TileType, p.X, p.Y, Main.spriteBatch);
		}

		if (solidLayer) {
			Main.spriteBatch.End();
		}
	}
}
