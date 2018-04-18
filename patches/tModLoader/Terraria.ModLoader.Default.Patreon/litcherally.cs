namespace Terraria.ModLoader.Default.Patreon
{
	class litcherally_Head : PatreonItem
	{
		public override string PatreonName => "litcherally";
		public override PatreonItemType PatreonEquipType => PatreonItemType.Head;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 22;
			item.height = 22;
		}
	}

	class litcherally_Body : PatreonItem
	{
		public override string PatreonName => "litcherally";
		public override PatreonItemType PatreonEquipType => PatreonItemType.Body;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 30;
			item.height = 20;
		}
	}

	class litcherally_Legs : PatreonItem
	{
		public override string PatreonName => "litcherally";
		public override PatreonItemType PatreonEquipType => PatreonItemType.Legs;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}
	}
}
