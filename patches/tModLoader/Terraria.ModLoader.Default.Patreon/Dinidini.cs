namespace Terraria.ModLoader.Default.Patreon
{
	internal class dinidini_Head : PatreonItem
	{
		public override string PatreonName => "dinidini";
		public override EquipType PatreonEquipType => EquipType.Head;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 28;
			item.height = 20;
		}
	}

	internal class dinidini_Body : PatreonItem
	{
		public override string PatreonName => "dinidini";
		public override EquipType PatreonEquipType => EquipType.Body;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 28;
			item.height = 24;
		}
	}

	internal class dinidini_Legs : PatreonItem
	{
		public override string PatreonName => "dinidini";
		public override EquipType PatreonEquipType => EquipType.Legs;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}
	}

	internal class dinidini_Wings : PatreonItem
	{
		public override string PatreonName => "dinidini";
		public override EquipType PatreonEquipType => EquipType.Wings;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.vanity = false;
			item.width = 24;
			item.height = 8;
			item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.wingTimeMax = 150;
		}
	}
}
