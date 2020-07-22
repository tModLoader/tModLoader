using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleDye : ModItem
	{
		//PlayerHooks.GetDyeTraderReward(this, list);
		public override void SetDefaults() {
			// item.dye is already assigned to this item prior to SetDefaults because of the GameShaders.Armor.BindShader code in ExampleMod.Load. 
			// This code here remembers item.dye so that information isn't lost during CloneDefaults.
			byte dye = item.dye;
			item.CloneDefaults(ItemID.GelDye);
			item.dye = dye;
		}
	}
}