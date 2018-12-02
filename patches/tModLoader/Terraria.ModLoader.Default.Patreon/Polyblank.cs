namespace Terraria.ModLoader.Default.Patreon
{
	internal class Polyblank_Head : PatreonItem
	{
		public override string SetName => "Polyblank";
		public override EquipType ItemEquipType => EquipType.Head;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 24;
			item.height = 22;
		}
	}

	internal class Polyblank_Body : PatreonItem
	{
		public override string SetName => "Polyblank";
		public override EquipType ItemEquipType => EquipType.Body;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 30;
			item.height = 20;
		}
	}

	internal class Polyblank_Legs : PatreonItem
	{
		public override string SetName => "Polyblank";
		public override EquipType ItemEquipType => EquipType.Legs;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}
	}
}
