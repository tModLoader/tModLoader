using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class PurityShield : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Shield of Purity");
			Tooltip.SetDefault("WIP until bluemagic123 learns how to make mounts");
		}

		public override void SetDefaults()
		{
			item.width = 32;
			item.height = 32;
			item.useTime = 20;
			item.useAnimation = 20;
			item.useStyle = 1;
			item.value = Item.sellPrice(2, 0, 0, 0);
			item.rare = 11;
			item.UseSound = SoundID.Item79;
			item.noMelee = true;
		}
	}
}