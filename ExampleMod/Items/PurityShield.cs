using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class PurityShield : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Shield of Purity";
			item.width = 32;
			item.height = 32;
			item.toolTip = "WIP until bluemagic123 learns how to make mounts";
			item.useTime = 20;
			item.useAnimation = 20;
			item.useStyle = 1;
			item.value = Item.sellPrice(2, 0, 0, 0);
			item.rare = 11;
			item.useSound = 79;
			item.noMelee = true;
		}
	}
}