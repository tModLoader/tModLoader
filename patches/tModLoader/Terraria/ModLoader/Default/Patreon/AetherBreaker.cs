using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class AetherBreaker_Head : PatreonItem
{
}

// An extra hat requested for a friend
[AutoloadEquip(EquipType.Head)]
internal class WitchDaggah_Head : PatreonItem
{
}

[AutoloadEquip(EquipType.Body)]
internal class AetherBreaker_Body : PatreonItem
{
}

[AutoloadEquip(EquipType.Legs)]
internal class AetherBreaker_Legs : PatreonItem
{
}

[AutoloadEquip(EquipType.Wings)]
internal class AetherBreaker_Wings : PatreonItem
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
