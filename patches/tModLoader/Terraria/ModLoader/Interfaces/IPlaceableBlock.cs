using System;
using System.Collections.Generic;
using System.Text;

namespace Terraria.ModLoader.Interfaces;

internal interface IPlaceableBlock
{
	/// <summary>
	/// Allows you to stop this tile/wall from being placed at the given coordinates. This method is called on the local client.
	/// <para/> For tiles this is also checked during block replacement, but <see cref="ModTile.CanReplace(int, int, int)"/> should be used for replace-specific logic.
	/// <para/> Return false to stop the tile/wall from being placed. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	bool CanPlace(int i, int j);

	/// <summary>
	/// Allows you to do something when this tile/wall is placed. Called on the local Client and Single Player.
	/// <para/> Note that the coordinates in this method account for the placement origin and are not necessarily the coordinates of the top left tile of a multi-tile.
	/// </summary>
	/// <param name="i">The x position in tile coordinates. Equal to Player.tileTargetX</param>
	/// <param name="j">The y position in tile coordinates. Equal to Player.tileTargetY</param>
	/// <param name="item">The item used to place this tile/wall.</param>
	void PlaceInWorld(int i, int j, Item item);
}
