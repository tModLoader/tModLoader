namespace Terraria.ModLoader.Default.Patreon
{
	internal class KittyKitCatCat_Head : PatreonItem
	{
		public override string SetName => "KittyKitCatCat";
		public override EquipType ItemEquipType => EquipType.Head;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 34;
			item.height = 22;
		}
	}

	internal class KittyKitCatCat_Body : PatreonItem
	{
		public override string SetName => "KittyKitCatCat";
		public override EquipType ItemEquipType => EquipType.Body;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 42;
			item.height = 24;
		}
	}

	internal class KittyKitCatCat_Legs : PatreonItem
	{
		public override string SetName => "KittyKitCatCat";
		public override EquipType ItemEquipType => EquipType.Legs;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}
	}
}
