namespace Terraria.ModLoader.Default.Patreon
{
	internal class Orian_Head : PatreonItem
	{
		public override string SetName => "Orian";
		public override EquipType ItemEquipType => EquipType.Head;

		public override bool IsVanitySet(int head, int body, int legs) {
			return head == Mod.GetEquipSlot($"{SetName}_{EquipType.Head}", EquipType.Head)
				   && body == Mod.GetEquipSlot($"{SetName}_{EquipType.Body}", EquipType.Body)
				   && legs == Mod.GetEquipSlot($"{SetName}_{EquipType.Legs}", EquipType.Legs);
		}

		public override void UpdateVanitySet(Player player) {
			PatronModPlayer.Player(player).OrianSet = true;
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 24;
			item.height = 24;
		}
	}

	internal class Orian_Body : PatreonItem
	{
		public override string SetName => "Orian";
		public override EquipType ItemEquipType => EquipType.Body;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 30;
			item.height = 20;
		}
	}

	internal class Orian_Legs : PatreonItem
	{
		public override string SetName => "Orian";
		public override EquipType ItemEquipType => EquipType.Legs;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}

		public override void EquipFrameEffects(Player player, EquipType type)
		{
			player.shoe = 0;
		}
	}
}
