namespace Terraria.ModLoader.Default.Patreon
{
	class KittyKitCatCat_Head : PatreonItem
	{
		public override string PatreonName => "KittyKitCatCat";
		public override PatreonItemType PatreonEquipType => PatreonItemType.Head;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 34;
			item.height = 22;
		}
	}

	class KittyKitCatCat_Body : PatreonItem
	{
		public override string PatreonName => "KittyKitCatCat";
		public override PatreonItemType PatreonEquipType => PatreonItemType.Body;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 42;
			item.height = 24;
		}
	}

	class KittyKitCatCat_Legs : PatreonItem
	{
		public override string PatreonName => "KittyKitCatCat";
		public override PatreonItemType PatreonEquipType => PatreonItemType.Legs;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}
	}
}
