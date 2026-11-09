using Microsoft.Xna.Framework;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon
{
	[AutoloadEquip(EquipType.Head)]
	internal class Frosty_Hat : PatreonItem
	{
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();

			ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
		}
    }

	[AutoloadEquip(EquipType.Body)]
	internal class Frosty_Hoodie : PatreonItem
	{
	}

	[AutoloadEquip(EquipType.Legs)]
	internal class Frosty_Pants : PatreonItem
	{
	}

	internal class FrostyPlayer : ModPlayer
	{
		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
			if (drawInfo.drawPlayer.body == EquipLoader.GetEquipSlot(Mod, "Frosty_Hoodie", EquipType.Body)) {
				drawInfo.colorArmorBody = drawInfo.colorUnderShirt;
			}
		}
	}
}
