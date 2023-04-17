using Terraria;
using Terraria.ModLoader;

public class ModPylonTest : ModPylon
{
#if COMPILE_ERROR
	public override NPCShop.Entry GetNPCShopEntry()/* tModPorter See ExamplePylonTile for an example. To register to specific NPC shops, use the new shop system directly in ModNPC.AddShop, GlobalNPC.ModifyShop or ModSystem.PostAddRecipes */
	{
		return isNPCHappyEnough ? ItemDropOverride : null;
	}
#endif
}