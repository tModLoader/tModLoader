using System;
using System.Collections.Generic;
using System.Text;
using Terraria.ID;

namespace Terraria.ModLoader.Interfaces;

internal interface IWorldUpdateBlock
{
	/// <summary>
	/// Called whenever the world randomly decides to update this tile/wall in a given tick. Useful for things such as growing or spreading.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="underground">Whether the tile/wall is considered underground. This usually means below <see cref="Main.worldSurface"/>, but if the "Don't dig up" special world seed (<see cref="Main.remixWorld"/>) is active tiles technically underground might be considered overground for random update purposes. </param>
	void RandomUpdate(int i, int j, bool underground);

	/// <summary>
	/// Allows you to change what happens when this tile/wall is converted into another biome. If you need to override or add a new conversion to a vanilla tile, use <see cref="TileLoader.RegisterConversion"/> and <see cref="WallLoader.RegisterConversion"/>.
	/// <para/> Purification powder uses a separate conversionType, as it doesn't convert hallowed tiles back to purity tiles. Be sure to check for <see cref="BiomeConversionID.PurificationPowder"/> as well as <see cref="BiomeConversionID.Purity"/> when handling corruption/crimson tiles.
	/// <para/> You can use <see cref="WorldGen.ConvertTile(int, int, int)"/> or <see cref="WorldGen.ConvertWall"/> to automatically handle tile framing and multiplayer syncing.
	/// </summary>
	/// <param name="i"></param>
	/// <param name="j"></param>
	/// <param name="conversionType">The <see cref="BiomeConversionID"/> of the conversion</param>
	void Convert(int i, int j, int conversionType);
}
