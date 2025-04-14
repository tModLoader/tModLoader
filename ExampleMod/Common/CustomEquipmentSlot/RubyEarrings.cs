using Terraria.Enums;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.CustomEquipmentSlot
{
	[AutoloadEquip_Earring]
	public class RubyEarrings : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToAccessory(16, 16);
			Item.SetShopValues(ItemRarityColor.Orange3, Item.buyPrice(gold: 6));
			Item.vanity = true;
		}
	}
}
