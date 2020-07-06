using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class PurityShield : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Shield of Purity");
		}

		public override void SetDefaults() {
			item.width = 32;
			item.height = 32;
			item.useTime = 20;
			item.useAnimation = 20;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.value = Item.sellPrice(2, 0, 0, 0);
			item.rare = ItemRarityID.Purple;
			item.UseSound = SoundID.Item79;
			item.noMelee = true;
		}
	}
}