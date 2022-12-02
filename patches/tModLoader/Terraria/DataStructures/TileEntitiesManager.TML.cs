using System.Collections.Generic;

namespace Terraria.DataStructures;

public partial class TileEntitiesManager
{
	/// <summary> Gets the template TileEntity object with the given id (not the new instance which gets added to the world as the game is played). This method will throw exceptions on failure. </summary>
	/// <exception cref="KeyNotFoundException"/>
	public TileEntity GetTileEntity<T>(int id) where T : TileEntity
		=> _types[id] as T;

	/// <summary> Attempts to get the template TileEntity object with the given id (not the new instance which gets added to the world as the game is played). </summary>
	public bool TryGetTileEntity<T>(int id, out T tileEntity) where T : TileEntity
	{
		if (!_types.TryGetValue(id, out var entity)) {
			tileEntity = default;

			return false;
		}

		return (tileEntity = entity as T) != null;
	}

	public IReadOnlyDictionary<int, TileEntity> EnumerateEntities() => _types;

	internal void Reset()
	{
		_types.Clear();

		_nextEntityID = 0;

		RegisterAll();
	}
}
