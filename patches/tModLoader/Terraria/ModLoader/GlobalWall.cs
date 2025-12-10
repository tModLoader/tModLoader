using Microsoft.Xna.Framework;

namespace Terraria.ModLoader;

/// <summary>
/// This class allows you to modify the behavior of any wall in the game (although admittedly walls don't have much behavior).
/// <br/> To use it, simply create a new class deriving from this one. Implementations will be registered automatically.
/// </summary>
public abstract class GlobalWall : GlobalBlockType
{
	protected sealed override void Register()
	{
		ModTypeLookup<GlobalWall>.Register(this);
		WallLoader.globalWalls.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// Allows you to customize which items the wall at the given coordinates drops. Return false to stop the game from dropping the wall's default item (the dropType parameter). Returns true by default.
	/// </summary>
	public virtual bool Drop(int i, int j, int type, ref int dropType)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine what happens when the wall at the given coordinates is killed or hit with a hammer. Fail determines whether the wall is mined (whether it is killed).
	/// </summary>
	public virtual void KillWall(int i, int j, int type, ref bool fail)
	{
	}

	/// <summary>
	/// Called for every wall that updates due to being placed or being next to a wall that is changed. Return false to stop the game from carrying out its default WallFrame operations. If you return false, make sure to set <see cref="Tile.WallFrameNumber"/>, <see cref="Tile.WallFrameX"/>, and <see cref="Tile.WallFrameY"/> according to the your desired custom framing design. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type">Type of the wall being framed</param>
	/// <param name="randomizeFrame">True if the calling code intends that the frameNumber be randomly changed, such as when placing the wall initially or loading the world, but not when updating due to nearby tile or wall placements</param>
	/// <param name="style">The style or orientation that will be applied</param>
	/// <param name="frameNumber">The random style that will be applied</param>
	/// <returns></returns>
	public virtual bool WallFrame(int i, int j, int type, bool randomizeFrame, ref int style, ref int frameNumber)
	{
		return true;
	}

	/// <inheritdoc cref="ModPlayer.CanBeTeleportedTo(Vector2, string)"/>
	public virtual bool CanBeTeleportedTo(int i, int j, int type, Player player, string context)
	{
		return true;
	}

	/// <inheritdoc cref="ModWall.OnWallConverted(int, int, int, int, int)"/>
	public virtual void OnWallConverted(int i, int j, int fromType, int toType, int conversionType)
	{
	}
}
