using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	[AutoloadEquip(EquipType.Head)]
	internal class PowerRanger_Head : AndromedonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();

			Item.Size = new Vector2(18, 20);
		}

		public override bool IsVanitySet(int head, int body, int legs)
			=> head == Mod.GetEquipSlot(nameof(PowerRanger_Head), EquipType.Head)
			&& body == Mod.GetEquipSlot(nameof(PowerRanger_Body), EquipType.BodyLegacy)
			&& legs == Mod.GetEquipSlot(nameof(PowerRanger_Legs), EquipType.Legs);

		public override void UpdateVanitySet(Player player)
			=> player.GetModPlayer<DeveloperPlayer>().AndromedonEffect.HasSetBonus = true;
	}
}
