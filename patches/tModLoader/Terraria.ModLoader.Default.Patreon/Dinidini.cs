namespace Terraria.ModLoader.Default.Patreon
{
	internal class dinidini_Head : PatreonItem
	{
		public override string SetName => "dinidini";
		public override EquipType ItemEquipType => EquipType.Head;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 28;
			item.height = 20;
		}
	}

	internal class dinidini_Body : PatreonItem
	{
		public override string SetName => "dinidini";
		public override EquipType ItemEquipType => EquipType.Body;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 28;
			item.height = 24;
		}
	}

	internal class dinidini_Legs : PatreonItem
	{
		public override string SetName => "dinidini";
		public override EquipType ItemEquipType => EquipType.Legs;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}
	}

	internal class dinidini_Wings : PatreonItem
	{
		public override string SetName => "dinidini";
		public override EquipType ItemEquipType => EquipType.Wings;

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
