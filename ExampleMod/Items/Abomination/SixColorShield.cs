using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ExampleMod.Items.Abomination
{
	public class SixColorShield : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Six-Color Shield";
			item.width = 24;
			item.height = 24;
			item.toolTip = "Creates elemental energy to protect you when damaged.";
			item.value = Item.buyPrice(0, 10, 0, 0);
			item.rare = 9;
			item.expert = true;
			item.accessory = true;
			item.damage = 120;
			item.magic = true;
			item.knockBack = 2f;
			item.defense = 6;
		}

		public override DrawAnimation GetAnimation()
		{
			return new DrawAnimationVertical(10, 4);
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
            player.GetModPlayer<ExamplePlayer>(mod).elementShield = true;
		}

		public override Color? GetAlpha(Color lightColor)
		{
			return Color.White;
		}
	}
}