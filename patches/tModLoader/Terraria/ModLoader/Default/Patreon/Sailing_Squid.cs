using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class Sailing_Squid_Head : PatreonItem
{
}

[AutoloadEquip(EquipType.Body)]
internal class Sailing_Squid_Body : PatreonItem
{
}

[AutoloadEquip(EquipType.Legs)]
internal class Sailing_Squid_Legs : PatreonItem
{
}

[AutoloadEquip(EquipType.Wings)]
internal class Sailing_Squid_Wings : PatreonItem
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
