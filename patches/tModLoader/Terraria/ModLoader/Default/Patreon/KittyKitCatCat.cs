namespace Terraria.ModLoader.Default.Patreon
{
	[AutoloadEquip(EquipType.Head)]
	internal class KittyKitCatCat_Head : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 34;
			item.height = 22;
		}
	}

	[AutoloadEquip(EquipType.Body)]
	internal class KittyKitCatCat_Body : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 42;
			item.height = 24;
		}
	}

	[AutoloadEquip(EquipType.Legs)]
	internal class KittyKitCatCat_Legs : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}
	}
}
