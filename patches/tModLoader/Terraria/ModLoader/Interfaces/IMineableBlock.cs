using System;
using System.Collections.Generic;
using System.Text;

namespace Terraria.ModLoader.Interfaces;

internal interface IMineableBlock
{

	/// <summary>
	/// Allows you to customize which sound you want to play when the tile/wall at the given coordinates is hit. Return false to stop the game from playing its default sound for the tile/wall. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="fail">If true, the tile/wall is only partially damaged. If false, the tile/wall is fully destroyed.</param>
	public bool KillSound(int i, int j, bool fail);

	/// <summary>
	/// Allows you to change how many dust particles are created when the tile/wall at the given coordinates is hit.
	/// <para/> Use <see cref="CreateDust(int, int, ref int)"/> to customize the dust spawned.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="fail">If true, the tile is spawning dust for reasons other than the tile actually being destroyed. Worms, projectiles, and other effects cause dust to spawn aside from the usual case of the tile breaking.</param>
	/// <param name="num">The number of dust that will be spawned by the calling code</param>
	public void NumDust(int i, int j, bool fail, ref int num);

	/// <summary>
	/// Allows you to modify the default type of dust created when the tile/wall at the given coordinates is hit. Return false to stop the default dust (the type parameter) from being created. Returns true by default.
	/// <para/> The <paramref name="type"/> parameter defaults to <see cref="DustType"/>.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type">The dust type that will be spawned by the calling code</param>
	public bool CreateDust(int i, int j, ref int type);

	/// <summary>
	/// Whether or not the tile/wall at the given coordinates can be killed by an explosion (ie. bombs). Returns true by default; return false to stop an explosion from destroying it.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public bool CanExplode(int i, int j);
}
