namespace Terraria.GameContent.Drawing
{
	public partial class TileDrawing
	{
		public static bool IsTileDangerous(int tileX, int tileY, Player localPlayer) {
			Tile tile = Main.tile[tileX, tileY];
			return IsTileDangerous(tileX, tileY, localPlayer, tile, tile.type);
		}
	}
}
