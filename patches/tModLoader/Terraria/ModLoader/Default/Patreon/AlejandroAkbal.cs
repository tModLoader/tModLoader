namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class AlejandroAkbal_Head : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 34;
		Item.height = 22;
	}
}

[AutoloadEquip(EquipType.Body)]
internal class AlejandroAkbal_Body : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 42;
		Item.height = 24;
	}
}

[AutoloadEquip(EquipType.Legs)]
internal class AlejandroAkbal_Legs : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 22;
		Item.height = 18;
	}
}

[AutoloadEquip(EquipType.Back)]
internal class AlejandroAkbal_Back : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 22;
		Item.height = 18;
		Item.accessory = true;
	}
}
