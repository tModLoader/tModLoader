namespace Terraria.ModLoader.Default.Patreon
{
	[AutoloadEquip(EquipType.Head)]
	internal class dinidini_Head : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 28;
			item.height = 20;
		}
	}

	[AutoloadEquip(EquipType.Body)]
	internal class dinidini_Body : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 28;
			item.height = 24;
		}
	}

	[AutoloadEquip(EquipType.Legs)]
	internal class dinidini_Legs : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}
	}

	[AutoloadEquip(EquipType.Wings)]
	internal class dinidini_Wings : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			item.vanity = false;
			item.width = 24;
			item.height = 8;
			item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.wingTimeMax = 150;
		}
	}
}
