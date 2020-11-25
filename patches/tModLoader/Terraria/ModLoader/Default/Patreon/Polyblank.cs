namespace Terraria.ModLoader.Default.Patreon
{
	[AutoloadEquip(EquipType.Head)]
	internal class Polyblank_Head : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 24;
			item.height = 22;
		}
	}

	[AutoloadEquip(EquipType.Body)]
	internal class Polyblank_Body : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 30;
			item.height = 20;
		}
	}

	[AutoloadEquip(EquipType.Legs)]
	internal class Polyblank_Legs : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}
	}
}
