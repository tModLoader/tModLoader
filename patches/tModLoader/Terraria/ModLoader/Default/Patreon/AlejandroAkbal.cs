namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class AlejandroAkbal_Head : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
	}
}

[AutoloadEquip(EquipType.Body)]
internal class AlejandroAkbal_Body : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
	}
}

[AutoloadEquip(EquipType.Legs)]
internal class AlejandroAkbal_Legs : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
	}
}

[AutoloadEquip(EquipType.Back)]
internal class AlejandroAkbal_Back : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.accessory = true;
	}
}
