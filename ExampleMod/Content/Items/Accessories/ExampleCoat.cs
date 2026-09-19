using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	// Coat equip textures use the same 360x224 composite layout as Body equip textures,
	// but draw over the body armor from an accessory or vanity accessory slot.
	[AutoloadEquip(EquipType.Coat)]
	public class ExampleCoat : ModItem
	{
		// Optional 40x1120 lower extensions are autoloaded from ExampleCoat_Coat_Front.png
		// and ExampleCoat_Coat_Back.png. Neither file is required for coats without a lower extension.
		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 30;
			Item.accessory = true;
			Item.vanity = true;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
	}
}
