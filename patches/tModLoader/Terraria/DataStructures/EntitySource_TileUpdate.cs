#nullable enable

namespace Terraria.DataStructures
{
	/// <summary>
	/// Added by TML. Derives from <see cref="AEntitySource_Tile"/>.
	/// </summary>
	public class EntitySource_TileUpdate : AEntitySource_Tile
	{
		public EntitySource_TileUpdate(int tileCoordsX, int tileCoordsY, string? context = null)
			: base(tileCoordsX, tileCoordsY, context) {
		}
	}
}
