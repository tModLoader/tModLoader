namespace Terraria;

public partial class WorldSections
{
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