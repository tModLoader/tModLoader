using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Items.Armor
{
	public class PuritySpiritMask : ModItem
	{
		public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
		{
			equips.Add(EquipType.Head);
			return true;
		}

		public override void SetDefaults()
		{
			item.name = "Spirit of Purity Mask";
			item.width = 18;
			item.height = 18;
			item.rare = 1;
			item.vanity = true;
		}

		public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor)
		{
			color = drawPlayer.GetImmuneAlphaPure(Color.White, shadow);
		}
	}
}