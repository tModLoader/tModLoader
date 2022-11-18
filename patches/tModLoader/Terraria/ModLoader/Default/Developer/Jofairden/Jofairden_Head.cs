using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Developer.Jofairden;

[LegacyName("PowerRanger_Head")]
[AutoloadEquip(EquipType.Head)]
internal class Jofairden_Head : JofairdenArmorItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new Vector2(18, 20);
	}

	public override bool IsVanitySet(int head, int body, int legs)
		=> head == EquipLoader.GetEquipSlot(Mod, nameof(Jofairden_Head), EquipType.Head)
		&& body == EquipLoader.GetEquipSlot(Mod, nameof(Jofairden_Body), EquipType.Body)
		&& legs == EquipLoader.GetEquipSlot(Mod, nameof(Jofairden_Legs), EquipType.Legs);

	public override void UpdateVanitySet(Player player)
		=> player.GetModPlayer<JofairdenArmorEffectPlayer>().HasSetBonus = true;
}
