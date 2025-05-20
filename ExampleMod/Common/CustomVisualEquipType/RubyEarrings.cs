using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace ExampleMod.Common.CustomVisualEquipType
{
	// A vanity accessory showcasing our custom earring EquipType
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
