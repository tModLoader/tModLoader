using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class POCKETS_Head : PatreonItem
{
}

[AutoloadEquip(EquipType.Body)]
internal class POCKETS_Body : PatreonItem
{
}

[AutoloadEquip(EquipType.Legs)]
internal class POCKETS_Legs : PatreonItem
{
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
		Item.accessory = true;
	}
}
