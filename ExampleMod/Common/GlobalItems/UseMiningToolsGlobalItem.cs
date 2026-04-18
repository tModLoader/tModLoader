using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalItems
{
	internal class UseMiningToolsGlobalItem : GlobalItem
	{
		public override bool MiningUsageCondition(Player player, Item item, int targetX, int targetY, bool origConditionValue) {
			// can Gravedigger's Shovel digging
			if (item.type == ItemID.GravediggerShovel) {
				return false;
			}
			return origConditionValue;
		}

		public override bool IsAValidTool(Player player, Item item, bool isAValidTool) {
			// can Gravedigger's Shovel digging
			if (item.type == ItemID.GravediggerShovel) {
				return false;
			}
			return isAValidTool;
		}
	}
}
