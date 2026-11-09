using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class dschosen_Head : PatreonItem
{
}

[AutoloadEquip(EquipType.Body)]
internal class dschosen_Body : PatreonItem
{
}

[AutoloadEquip(EquipType.Legs)]
internal class dschosen_Legs : PatreonItem
{
}

[AutoloadEquip(EquipType.Wings)]
internal class dschosen_Wings : PatreonItem
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
