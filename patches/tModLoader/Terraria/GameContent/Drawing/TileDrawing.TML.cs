using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace Terraria.GameContent.Drawing;

public partial class TileDrawing
{
	// TML(liquid-slopes)
	/// <summary>
	/// Cached data for rendering liquids on solid tiles at the edge of liquid
	/// pools.
	/// </summary>
	public struct LiquidEdgeCache
	{
		/// <summary>
		/// The X coordinate of the tile.
		/// </summary>
		public int TileX;

		/// <summary>
		/// The Y coordinate of the tile.
		/// </summary>
		public int TileY;

		/// <summary>
		/// The actual liquid type to be rendered.
		/// </summary>
		public int LiquidType;

		/// <summary>
		/// The cosmetic style to be rendered (such as water styles).
		/// </summary>
		public int LiquidStyle;

		/// <summary>
		/// The additional offset to apply based which edge is being drawn.
		/// </summary>
		public Vector2 LiquidPosition;

		/// <summary>
		/// The framing of the liquid slope.
		/// </summary>
		public Rectangle LiquidFrame;

		/// <summary>
		/// Whether this expects the liquid to be rendered with water biome
		/// fading.
		/// </summary>
		public bool IsWaterForFading;
	}

	/// <summary>
	/// The wind grid used to exert wind effects on tiles.
	/// </summary>
	public WindGrid Wind => _windGrid;

	// TML(liquid-slopes)
	/// <summary>
	/// Tiles which mask rendered liquid (tiles on the edge of bodies of
	/// liquid).
	/// </summary>
	public Dictionary<Point, LiquidEdgeCache> LiquidEdges { get; } = [];

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
