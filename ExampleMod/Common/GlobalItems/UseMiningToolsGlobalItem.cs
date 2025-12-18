using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalItems
{
	internal class UseMiningToolsGlobalItem : GlobalItem
	{
		public override void UseMiningTools(Item item, Player player, ref Player.SpecialToolUsageSettings usageSettings) {
			base.UseMiningTools(item, player, ref usageSettings);

			// can Gravedigger's Shovel digging
			if (item.type == ItemID.GravediggerShovel) {
				usageSettings.UsageCondition += (_, _, _, _) => false;
			}
		}
	}
}
