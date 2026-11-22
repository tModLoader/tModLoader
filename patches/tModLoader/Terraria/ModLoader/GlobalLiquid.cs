using Terraria.Graphics.Light;

namespace Terraria.ModLoader;
/// <summary>
/// This class allows you to modify the behavior of any liquid in the game, both vanilla and modded.
/// <br /> To use it, simply create a new class deriving from this one. Implementations will be registered automatically.
/// </summary>
public abstract class GlobalLiquid : ModType
{
	protected sealed override void Register()
	{
		ModTypeLookup<GlobalLiquid>.Register(this);
		LiquidLoader.globalLiquids.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// Allows you to determine how much light this liquid emits.<br />
	/// It can also let you light up the block in front of this liquid.<br />
	/// See <see cref="TileLightScanner.ApplyLiquidLight(Tile, int, int, ref Vector3)" /> for vanilla tile light values to use as a reference.<br />
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <param name="r">The red component of light, usually a value between 0 and 1</param>
	/// <param name="g">The green component of light, usually a value between 0 and 1</param>
	/// <param name="b">The blue component of light, usually a value between 0 and 1</param>
	public virtual void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b)
	{
	}
}
