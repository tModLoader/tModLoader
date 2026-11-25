using Terraria.GameContent.Liquid;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default;

public class UnloadedLiquid : ModLiquid
{
	public override string Texture => "ModLoader/UnloadedLiquid";

	public override void SetStaticDefaults()
	{
		LiquidRenderer.WATERFALL_LENGTH[Type] = 1; // makes unloaded floating liquids look less odd
		TileIO.Liquids.unloadedTypes.Add(Type);
	}

	public bool SettleLiquidMovement(int i, int j) => false; // prevents unloaded liquids from being moved when loading worlds

	public bool PreLiquidMovement(int x, int y, Liquid liquidCache) => false; // prevents unloaded liquids from moving

	public bool CanPump(int inletX, int inletY, int outletX, int outletY) => false; // prevents pumping
}
