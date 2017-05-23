using System.Collections.Generic;
using Terraria.ModLoader;

namespace ExampleMod.Items.Armor
{
	public class AbominationMask : ModItem
	{
		public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
		{
			equips.Add(EquipType.Head);
			return true;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("AbominationMask");
		}

		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 20;
			item.rare = 1;
			item.vanity = true;
		}

		public override bool DrawHead()
		{
			return false;
		}
	}
}