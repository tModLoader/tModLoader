using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class Remeus_Head : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new Vector2(34);
	}
}

[AutoloadEquip(EquipType.Body)]
internal class Remeus_Body : PatreonItem
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		ArmorIDs.Body.Sets.HidesTopSkin[Item.bodySlot] = true;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new Vector2(30, 18);
	}
}

[AutoloadEquip(EquipType.Legs)]
internal class Remeus_Legs : PatreonItem
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		ArmorIDs.Legs.Sets.HidesBottomSkin[Item.legSlot] = true;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new Vector2(22, 18);
	}
}