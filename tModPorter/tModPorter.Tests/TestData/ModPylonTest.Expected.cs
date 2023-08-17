using Terraria;
using Terraria.ModLoader;

public class ModPylonTest : ModPylon
{
#if COMPILE_ERROR
	public override NPCShop.Entry GetNPCShopEntry()/* tModPorter See ExamplePylonTile for an example. To register to specific NPC shops, use the new shop system directly in ModNPC.AddShop, GlobalNPC.ModifyShop or ModSystem.PostAddRecipes */
	{
		return isNPCHappyEnough ? ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ : null;
	}
#endif
}