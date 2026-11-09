using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class Polyblank_Head : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
	}
}

[AutoloadEquip(EquipType.Body)]
internal class Polyblank_Body : PatreonItem
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		ArmorIDs.Body.Sets.HidesTopSkin[Item.bodySlot] = true;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();
	}
}

[AutoloadEquip(EquipType.Legs)]
internal class Polyblank_Legs : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
	}
}
