using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class POCKETS_Head : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.Size = new Vector2(34);
	}
}

[AutoloadEquip(EquipType.Body)]
internal class POCKETS_Body : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.Size = new Vector2(30, 18);
	}
}

[AutoloadEquip(EquipType.Legs)]
internal class POCKETS_Legs : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.Size = new Vector2(22, 18);
	}
}

[AutoloadEquip(EquipType.Wings)]
internal class POCKETS_Wings : PatreonItem
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(150, 7f);
	}

	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.vanity = false;
		Item.width = 24;
		Item.height = 8;
		Item.accessory = true;
	}
}
