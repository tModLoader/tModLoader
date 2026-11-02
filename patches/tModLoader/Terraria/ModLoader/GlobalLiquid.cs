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
}
