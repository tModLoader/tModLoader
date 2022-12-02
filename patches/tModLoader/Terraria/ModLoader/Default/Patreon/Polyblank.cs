using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class Polyblank_Head : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 24;
		Item.height = 22;
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

		Item.width = 30;
		Item.height = 20;
	}
}

[AutoloadEquip(EquipType.Legs)]
internal class Polyblank_Legs : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 22;
		Item.height = 18;
	}
}
