using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class Polyblank_Head : PatreonItem
{
}

[AutoloadEquip(EquipType.Body)]
internal class Polyblank_Body : PatreonItem
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		ArmorIDs.Body.Sets.HidesTopSkin[Item.bodySlot] = true;
	}
}

[AutoloadEquip(EquipType.Legs)]
internal class Polyblank_Legs : PatreonItem
{
}
