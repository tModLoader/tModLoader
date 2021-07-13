namespace Terraria.ModLoader.Default.Patreon
{
	[AutoloadEquip(EquipType.Head)]
	internal class Orian_Head : PatreonItem
	{
		public override bool IsVanitySet(int head, int body, int legs) {
			return head == Mod.GetEquipSlot(nameof(Orian_Head), EquipType.Head)
				   && body == Mod.GetEquipSlot(nameof(Orian_Body), EquipType.BodyLegacy)
				   && legs == Mod.GetEquipSlot(nameof(Orian_Legs), EquipType.Legs);
		}

		public override void UpdateVanitySet(Player player) {
			PatronModPlayer.Player(player).OrianSet = true;
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Item.width = 24;
			Item.height = 24;
		}
	}

	[AutoloadEquip(EquipType.BodyLegacy)]
	internal class Orian_Body : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			Item.width = 30;
			Item.height = 20;
		}
	}

	[AutoloadEquip(EquipType.Legs)]
	internal class Orian_Legs : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			Item.width = 22;
			Item.height = 18;
		}

		public override void EquipFrameEffects(Player player, EquipType type)
		{
			player.shoe = 0;
		}
	}
}
