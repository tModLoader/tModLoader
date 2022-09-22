namespace Terraria.ModLoader.Default.Patreon
{
	[AutoloadEquip(EquipType.Head)]
	internal class Zeph_Head : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();

			Item.width = 34;
			Item.height = 22;
		}
	}

	[AutoloadEquip(EquipType.Body)]
	internal class Zeph_Body : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();

			Item.width = 42;
			Item.height = 24;
		}
	}

	[AutoloadEquip(EquipType.Legs)]
	internal class Zeph_Legs : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();

			Item.width = 22;
			Item.height = 18;
		}
	}

	[AutoloadEquip(EquipType.Wings)]
	internal class Zeph_Wings : PatreonItem
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
