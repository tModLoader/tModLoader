using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalItems
{
	internal class UseMiningToolsGlobalItem : GlobalItem
	{
		public override bool MiningUsageCondition(Player player, Item item, int targetX, int targetY) {
			// can Gravedigger's Shovel digging
			if (item.type == ItemID.GravediggerShovel) {
				return false;
			}
			return true;
		}

		public override bool IsAValidTool(Player player, Item item) {
			return false;
		}
	}
}
