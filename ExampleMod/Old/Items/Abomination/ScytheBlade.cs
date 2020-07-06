using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ExampleMod.Items.Abomination
{
	public class ScytheBlade : ModItem
	{
		public override void SetDefaults() {
			item.width = 20;
			item.height = 20;
			item.maxStack = 99;
			item.rare = ItemRarityID.Yellow;
			item.value = Item.sellPrice(0, 0, 50, 0);
		}
	}
}