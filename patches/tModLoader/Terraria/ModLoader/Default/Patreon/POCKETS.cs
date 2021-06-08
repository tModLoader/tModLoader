using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Patreon
{
	[AutoloadEquip(EquipType.Head)]
	internal class POCKETS_Head : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			Item.Size = new Vector2(34);
		}
	}

	[AutoloadEquip(EquipType.BodyLegacy)]
	internal class POCKETS_Body : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			Item.Size = new Vector2(30, 18);
		}
	}

	[AutoloadEquip(EquipType.Legs)]
	internal class POCKETS_Legs : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			Item.Size = new Vector2(22, 18);
		}
	}

	[AutoloadEquip(EquipType.Wings)]
	internal class POCKETS_Wings : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			Item.vanity = false;
			Item.width = 24;
			Item.height = 8;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.wingTimeMax = 150;
		}
	}
}