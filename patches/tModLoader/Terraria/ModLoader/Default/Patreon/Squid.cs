using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class Squid_Head : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.Size = new Vector2(26);
	}
}

[AutoloadEquip(EquipType.Body)]
internal class Squid_Body : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.Size = new Vector2(34, 26);
	}
}

[AutoloadEquip(EquipType.Legs)]
internal class Squid_Legs : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.Size = new Vector2(22, 18);
	}
}