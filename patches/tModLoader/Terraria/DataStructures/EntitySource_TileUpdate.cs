#nullable enable

namespace Terraria.DataStructures;

/// <summary>
/// Intended for mods to use when a tile spawns an entity due to periodic/random updating, rather than in response to a specific trigger.
/// </summary>
public class EntitySource_TileUpdate : AEntitySource_Tile
{
	public EntitySource_TileUpdate(int tileCoordsX, int tileCoordsY, string? context = null) : base(tileCoordsX, tileCoordsY, context) { }
}
