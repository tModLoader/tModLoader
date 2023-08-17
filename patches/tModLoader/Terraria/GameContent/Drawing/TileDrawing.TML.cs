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
}
