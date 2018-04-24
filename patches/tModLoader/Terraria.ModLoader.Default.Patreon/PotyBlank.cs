namespace Terraria.ModLoader.Default.Patreon
{
	class PotyBlank_Head : PatreonItem
	{
		public override string PatreonName => "PotyBlank";
		public override PatreonItemType PatreonEquipType => PatreonItemType.Head;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 24;
			item.height = 22;
		}
	}

	class PotyBlank_Body : PatreonItem
	{
		public override string PatreonName => "PotyBlank";
		public override PatreonItemType PatreonEquipType => PatreonItemType.Body;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 30;
			item.height = 20;
		}
	}

	class PotyBlank_Legs : PatreonItem
	{
		public override string PatreonName => "PotyBlank";
		public override PatreonItemType PatreonEquipType => PatreonItemType.Legs;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}
	}
}
