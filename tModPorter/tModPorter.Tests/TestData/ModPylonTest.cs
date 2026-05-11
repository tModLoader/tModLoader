using Terraria;
using Terraria.ModLoader;

public class ModPylonTest : ModPylon
{
	public override int? IsPylonForSale(int npcType, Player player, bool isNPCHappyEnough)
	{
		return isNPCHappyEnough ? ItemDrop : null;
	}

	public override bool ValidTeleportCheck_AnyDanger(TeleportPylonInfo pylonInfo)
	{
		return false;
	}
}

public class GlobalPylonTest : GlobalPylon
{
	public override bool? ValidTeleportCheck_PreAnyDanger(TeleportPylonInfo pylonInfo)
	{
		return null;
	}
}
