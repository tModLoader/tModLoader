using System.Collections.Generic;
using Terraria.ModLoader;

namespace ExampleMod.Items.Armor
{
	public class BunnyMask : ModItem
	{
		public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
		{
			equips.Add(EquipType.Head);
			return true;
		}

		public override void SetDefaults()
		{
			item.name = "Bunny Mask";
			item.width = 18;
			item.height = 18;
			item.rare = 1;
			item.vanity = true;
		}

		public override bool DrawHead()
		{
			return false;
		}
	}
}