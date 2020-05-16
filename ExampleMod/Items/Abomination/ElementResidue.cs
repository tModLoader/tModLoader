using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ExampleMod.Items.Abomination
{
	public class ElementResidue : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Residual Elements");
		}

		public override void SetDefaults() {
			item.width = 20;
			item.height = 20;
			item.maxStack = 99;
			item.rare = ItemRarityID.Cyan;
			item.value = Item.sellPrice(0, 2, 50, 0);
		}
	}
}