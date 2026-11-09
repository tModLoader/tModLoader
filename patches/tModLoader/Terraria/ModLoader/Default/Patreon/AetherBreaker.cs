using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class AetherBreaker_Head : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
	}
}

// An extra hat requested for a friend
[AutoloadEquip(EquipType.Head)]
internal class WitchDaggah_Head : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
	}
}

[AutoloadEquip(EquipType.Body)]
internal class AetherBreaker_Body : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
	}
}

[AutoloadEquip(EquipType.Legs)]
internal class AetherBreaker_Legs : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
	}
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
