namespace Terraria.ModLoader.Default.Patreon
{
	internal class Polyblank_Head : PatreonItem
	{
		public override string PatreonName => "Polyblank";
		public override EquipType PatreonEquipType => EquipType.Head;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 24;
			item.height = 22;
		}
	}

	internal class Polyblank_Body : PatreonItem
	{
		public override string PatreonName => "Polyblank";
		public override EquipType PatreonEquipType => EquipType.Body;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 30;
			item.height = 20;
		}
	}

	internal class Polyblank_Legs : PatreonItem
	{
		public override string PatreonName => "Polyblank";
		public override EquipType PatreonEquipType => EquipType.Legs;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}
	}
}
