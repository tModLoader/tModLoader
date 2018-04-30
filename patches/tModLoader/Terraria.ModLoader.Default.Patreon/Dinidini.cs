namespace Terraria.ModLoader.Default.Patreon
{
	class Dinidini_Head : PatreonItem
	{
		public override string PatreonName => "dinidini";
		public override PatreonItemType PatreonEquipType => PatreonItemType.Head;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 28;
			item.height = 20;
		}
	}

	class Dinidini_Body : PatreonItem
	{
		public override string PatreonName => "dinidini";
		public override PatreonItemType PatreonEquipType => PatreonItemType.Body;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 28;
			item.height = 24;
		}
	}

	class Dinidini_Legs : PatreonItem
	{
		public override string PatreonName => "dinidini";
		public override PatreonItemType PatreonEquipType => PatreonItemType.Legs;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}
	}
}
