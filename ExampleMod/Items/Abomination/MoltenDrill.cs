using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Items.Abomination
{
	//ported from my tAPI mod because I don't want to make more artwork
	public class MoltenDrill : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Molten Drill";
			item.damage = 40;
			item.melee = true;
			item.width = 20;
			item.height = 12;
			item.toolTip = "Can mine Lihzahrd Bricks";
			item.useTime = 7;
			item.useAnimation = 25;
			item.channel = true;
			item.noUseGraphic = true;
			item.noMelee = true;
			item.pick = 210;
			item.tileBoost++;
			item.useStyle = 5;
			item.knockBack = 6;
			item.value = Item.buyPrice(0, 22, 50, 0);
			item.rare = 9;
			item.useSound = 23;
			item.autoReuse = true;
			item.shoot = mod.ProjectileType("MoltenDrill");
			item.shootSpeed = 40f;
		}
	}
}