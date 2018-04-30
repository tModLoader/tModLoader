namespace Terraria.ModLoader.Default.Patreon
{
	class toplayz_Head : PatreonItem
	{
		public override string PatreonName => "toplayz";
		public override PatreonItemType PatreonEquipType => PatreonItemType.Head;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 28;
			item.height = 26;
		}
	}

	class toplayz_Body : PatreonItem
	{
		public override string PatreonName => "toplayz";
		public override PatreonItemType PatreonEquipType => PatreonItemType.Body;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 30;
			item.height = 20;
		}
	}

	class toplayz_Legs : PatreonItem
	{
		public override string PatreonName => "toplayz";
		public override PatreonItemType PatreonEquipType => PatreonItemType.Legs;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}
	}
}
