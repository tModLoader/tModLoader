using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class Glory_Head : PatreonItem
{
}

[AutoloadEquip(EquipType.Body)]
internal class Glory_Body : PatreonItem
{
}

[AutoloadEquip(EquipType.Legs)]
internal class Glory_Legs : PatreonItem
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		ArmorIDs.Legs.Sets.OverridesLegs[Item.legSlot] = true;
	}
}