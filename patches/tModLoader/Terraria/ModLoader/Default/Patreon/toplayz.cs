namespace Terraria.ModLoader.Default.Patreon
{
	[AutoloadEquip(EquipType.Head)]
	internal class toplayz_Head : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 28;
			item.height = 26;
		}
	}

	[AutoloadEquip(EquipType.Body)]
	internal class toplayz_Body : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 30;
			item.height = 20;
		}
	}

	[AutoloadEquip(EquipType.Legs)]
	internal class toplayz_Legs : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}
	}
}
