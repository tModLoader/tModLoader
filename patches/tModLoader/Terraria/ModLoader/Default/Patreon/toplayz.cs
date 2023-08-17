using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class toplayz_Head : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 28;
		Item.height = 26;
	}
}

[AutoloadEquip(EquipType.Body)]
internal class toplayz_Body : PatreonItem
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
internal class toplayz_Legs : PatreonItem
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		ArmorIDs.Legs.Sets.HidesBottomSkin[Item.legSlot] = true;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 22;
		Item.height = 18;
	}
}
