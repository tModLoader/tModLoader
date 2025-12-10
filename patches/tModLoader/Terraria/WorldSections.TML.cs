namespace Terraria;

public partial class WorldSections
{
	/// <summary>
	/// Checks if the tile at the specified tile coordinate has been loaded for this client in a multiplayer game session. In multiplayer, sections of tiles are sent when the player visits them, so much of the map will not be loaded for a client. until visited.
	/// <para/> Modders may need to check this for client code that could potentially access unloaded tiles.
	/// </summary>
	public bool TileLoaded(int tileX, int tileY)
	{
		return SectionLoaded(Netplay.GetSectionX(tileX), Netplay.GetSectionY(tileY));
	}

	public bool TilesLoaded(int startX, int startY, int endXInclusive, int endYInclusive)
	{
		int sX, sY, eX, eY;

		sX = Netplay.GetSectionX(startX);
		sY = Netplay.GetSectionY(startY);
		eX = Netplay.GetSectionX(endXInclusive);
		eY = Netplay.GetSectionY(endYInclusive);

		for (int x = sX; x <= eX; x++)
			for (int y = sY; y <= eY; y++)
				if (!SectionLoaded(x, y))
					return false;

		return true;
	}
}