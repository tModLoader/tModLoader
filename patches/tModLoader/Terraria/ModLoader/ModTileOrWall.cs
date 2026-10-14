using Terraria.Audio;
using Terraria.ID;

namespace Terraria.ModLoader;

/// <summary>
/// This is the superclass for <see cref="ModTile"/> and <see cref="ModWall"/>, combining common code
/// </summary>
public abstract class ModTileOrWall : ModBlockType
{
	/// <summary>
	/// The default style of sound made when this tile/wall is hit.<br/>
	/// Defaults to SoundID.Dig, which is the sound used for tiles such as dirt and sand.
	/// </summary>
	public SoundStyle? HitSound { get; set; } = SoundID.Dig;

	/// <summary> The default type of dust made when this tile/wall is hit.
	/// <para/> Defaults to 0, which is <see cref="DustID.Dirt"/>. To prevent spawning any hit dust, set this to -1 instead. </summary>
	public int DustType { get; set; }

	/// <summary>
	/// Called whenever the world randomly decides to update this tile/wall in a given tick. Useful for things such as growing or spreading.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="underground">Whether the tile/wall is considered underground. This usually means below <see cref="Main.worldSurface"/>, but if the "Don't dig up" special world seed (<see cref="Main.remixWorld"/>) is active tiles technically underground might be considered overground for random update purposes. </param>
	public virtual void RandomUpdate(int i, int j, bool underground) { }

	/// <summary>
	/// Allows you to change what happens when this tile/wall is converted into another biome. If you need to override or add a new conversion to a vanilla tile, use <see cref="TileLoader.RegisterConversion"/> and <see cref="WallLoader.RegisterConversion"/>.
	/// <para/> Purification powder uses a separate conversionType, as it doesn't convert hallowed tiles back to purity tiles. Be sure to check for <see cref="BiomeConversionID.PurificationPowder"/> as well as <see cref="BiomeConversionID.Purity"/> when handling corruption/crimson tiles.
	/// <para/> You can use <see cref="WorldGen.ConvertTile(int, int, int)"/> or <see cref="WorldGen.ConvertWall"/> to automatically handle tile framing and multiplayer syncing.
	/// </summary>
	/// <param name="i"></param>
	/// <param name="j"></param>
	/// <param name="conversionType">The <see cref="BiomeConversionID"/> of the conversion</param>
	public virtual void Convert(int i, int j, int conversionType) { }

	/// <summary>
	/// Allows you to stop this tile/wall from being placed at the given coordinates. This method is called on the local client.
	/// <para/> For tiles this is also checked during block replacement, but <see cref="ModTile.CanReplace(int, int, int)"/> should be used for replace-specific logic.
	/// <para/> Return false to stop the tile/wall from being placed. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual bool CanPlace(int i, int j)
	{
		return true;
	}

	/// <summary>
	/// Allows you to do something when this tile/wall is placed. Called on the local Client and Single Player.
	/// <para/> Note that the coordinates in this method account for the placement origin and are not necessarily the coordinates of the top left tile of a multi-tile.
	/// </summary>
	/// <param name="i">The x position in tile coordinates. Equal to Player.tileTargetX</param>
	/// <param name="j">The y position in tile coordinates. Equal to Player.tileTargetY</param>
	/// <param name="item">The item used to place this tile/wall.</param>
	public virtual void PlaceInWorld(int i, int j, Item item) { }

	/// <summary>
	/// Allows you to customize which sound you want to play when the tile/wall at the given coordinates is hit. Return false to stop the game from playing its default sound for the tile/wall. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="fail">If true, the tile/wall is only partially damaged. If false, the tile/wall is fully destroyed.</param>
	public virtual bool KillSound(int i, int j, bool fail)
	{
		return true;
	}

	/// <summary>
	/// Allows you to change how many dust particles are created when the tile/wall at the given coordinates is hit.
	/// <para/> Use <see cref="CreateDust(int, int, ref int)"/> to customize the dust spawned.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="fail">If true, the tile is spawning dust for reasons other than the tile actually being destroyed. Worms, projectiles, and other effects cause dust to spawn aside from the usual case of the tile breaking.</param>
	/// <param name="num">The number of dust that will be spawned by the calling code</param>
	public virtual void NumDust(int i, int j, bool fail, ref int num) { }

	/// <summary>
	/// Allows you to modify the default type of dust created when the tile/wall at the given coordinates is hit. Return false to stop the default dust (the type parameter) from being created. Returns true by default.
	/// <para/> The <paramref name="type"/> parameter defaults to <see cref="DustType"/>.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type">The dust type that will be spawned by the calling code</param>
	public virtual bool CreateDust(int i, int j, ref int type)
	{
		return true;
	}

	/// <summary>
	/// Whether or not the tile/wall at the given coordinates can be killed by an explosion (ie. bombs). Returns true by default; return false to stop an explosion from destroying it.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual bool CanExplode(int i, int j)
	{
		return true;
	}
}
