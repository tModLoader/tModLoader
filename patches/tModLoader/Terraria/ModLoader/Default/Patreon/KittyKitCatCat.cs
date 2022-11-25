namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class KittyKitCatCat_Head : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.width = 34;
		Item.height = 22;
	}
}

[AutoloadEquip(EquipType.Body)]
internal class KittyKitCatCat_Body : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.width = 42;
		Item.height = 24;
	}
}

[AutoloadEquip(EquipType.Legs)]
internal class KittyKitCatCat_Legs : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.width = 22;
		Item.height = 18;
	}
}
