using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal class PowerRanger_Head : AndromedonItem
	{
		public override EquipType ItemEquipType => EquipType.Head;

		public override void Load() {
			Mod.AddContent<AndromedonHeadGlow>();
			Mod.AddContent<AndromedonHeadShader>();
		}

		public override void SetDefaults() {
			base.SetDefaults();

			item.Size = new Vector2(18, 20);
		}

		public override bool IsVanitySet(int head, int body, int legs)
			=> head == Mod.GetEquipSlot($"{SetName}_{EquipType.Head}", EquipType.Head)
			&& body == Mod.GetEquipSlot($"{SetName}_{EquipType.Body}", EquipType.Body)
			&& legs == Mod.GetEquipSlot($"{SetName}_{EquipType.Legs}", EquipType.Legs);

		public override void UpdateVanitySet(Player player)
			=> player.GetModPlayer<DeveloperPlayer>().AndromedonEffect.HasSetBonus = true;
	}
}
