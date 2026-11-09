using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class Zeph_Head : PatreonItem
{
}

[AutoloadEquip(EquipType.Body)]
internal class Zeph_Body : PatreonItem
{
}

[AutoloadEquip(EquipType.Legs)]
internal class Zeph_Legs : PatreonItem
{
}

[AutoloadEquip(EquipType.Wings)]
internal class Zeph_Wings : PatreonItem
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
		Item.accessory = true;
	}
}
