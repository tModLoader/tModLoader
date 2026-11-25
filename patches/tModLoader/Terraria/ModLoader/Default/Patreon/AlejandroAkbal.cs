namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class AlejandroAkbal_Head : PatreonItem
{
}

[AutoloadEquip(EquipType.Body)]
internal class AlejandroAkbal_Body : PatreonItem
{
}

[AutoloadEquip(EquipType.Legs)]
internal class AlejandroAkbal_Legs : PatreonItem
{
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
