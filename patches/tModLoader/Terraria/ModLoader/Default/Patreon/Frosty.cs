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

		public override void SetDefaults() {
			base.SetDefaults();
		}
    }

	[AutoloadEquip(EquipType.Body)]
	internal class Frosty_Hoodie : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
		}
	}

	[AutoloadEquip(EquipType.Legs)]
	internal class Frosty_Pants : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
		}
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
