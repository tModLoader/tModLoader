using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	[LegacyName("PowerRanger_Head")]
	[AutoloadEquip(EquipType.Head)]
	internal class Jofairden_Head : JofairdenArmorItem
	{
		public override void SetDefaults() {
			base.SetDefaults();

			Item.Size = new Vector2(18, 20);
		}

		public override bool IsVanitySet(int head, int body, int legs)
			=> head == Mod.GetEquipSlot(nameof(Jofairden_Head), EquipType.Head)
			&& body == Mod.GetEquipSlot(nameof(Jofairden_Body), EquipType.Body)
			&& legs == Mod.GetEquipSlot(nameof(Jofairden_Legs), EquipType.Legs);

		public override void UpdateVanitySet(Player player)
			=> player.GetModPlayer<JofairdenArmorEffectPlayer>().HasSetBonus = true;
	}
}
