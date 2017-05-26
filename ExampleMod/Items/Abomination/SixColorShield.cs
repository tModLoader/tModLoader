using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ExampleMod.Items.Abomination
{
	public class SixColorShield : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Six-Color Shield");
			Tooltip.SetDefault("Creates elemental energy to protect you when damaged.");
			Main.RegisterItemAnimation(item.type, new DrawAnimationVertical(10, 4));
		}

		public override void SetDefaults()
		{
			item.width = 24;
			item.height = 24;
			item.value = Item.buyPrice(0, 10, 0, 0);
			item.rare = 9;
			item.expert = true;
			item.accessory = true;
			item.damage = 120;
			item.magic = true;
			item.knockBack = 2f;
			item.defense = 6;
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