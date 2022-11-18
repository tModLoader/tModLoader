using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class Coolmike5000_Head : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 34;
		Item.height = 22;
	}
}

[AutoloadEquip(EquipType.Body)]
internal class Coolmike5000_Body : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 42;
		Item.height = 24;
	}
}

[AutoloadEquip(EquipType.Legs)]
internal class Coolmike5000_Legs : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 22;
		Item.height = 18;
	}
}

[AutoloadEquip(EquipType.Wings)]
internal class Coolmike5000_Wings : PatreonItem
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(150, 7f);
	}
	
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.vanity = false;
		Item.width = 24;
		Item.height = 8;
		Item.accessory = true;
	}
}
