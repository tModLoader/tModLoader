using Terraria.ModLoader;

namespace ExampleMod.Items.Armor
{
	[AutoloadEquip(EquipType.Head)]
	public class BunnyMask : ModItem
	{
		public override void SetDefaults()
		{
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