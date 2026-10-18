using Terraria.GameContent.Liquid;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default;

public class UnloadedLiquid : ModLiquid
{
	public override string Texture => "ModLoader/UnloadedLiquid";

	public override void SetStaticDefaults()
	{
		LiquidRenderer.WATERFALL_LENGTH[Type] = 1; //makes unloaded floating liquids look less odd
		TileIO.Liquids.unloadedTypes.Add(Type);
	}
}