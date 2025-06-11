using Terraria;
using Terraria.ModLoader;

public class ModPylonTest : ModPylon
{
	public override int? IsPylonForSale(int npcType, Player player, bool isNPCHappyEnough)
	{
		return isNPCHappyEnough ? ItemDrop : null;
	}
}