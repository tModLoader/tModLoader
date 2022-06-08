using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class GlobalItemTest : GlobalItem
{
	public override bool? UseItem(Item item, Player player)/* tModPorter Suggestion: Return null instead of false */ => false;

	public override bool PreReforge(Item item) { return false; /* comment */ }

	public override void HoldStyle(Item item, Player player, Rectangle heldItemFrame) { /* comment */ }

	public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) { /* comment */ }

	public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)/* tModPorter Suggestion: Consider using new hook CanAccessoryBeEquippedWith */ { return true; /* comment */ }

	public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback) { /* Empty */ }

	public override void ModifyWeaponCrit(Item item, Player player, ref float crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) {
		damage += 0.1f;
		damage *= 0.2f;
		damage.Flat += 4;
	}

	public override void LoadData(Item item, TagCompound tag) { /* Empty */ }

#if COMPILE_ERROR
	public override void SaveData(Item item, TagCompound tag)/* tModPorter Suggestion: Edit tag parameter instead of returning new TagCompound */ => new TagCompound();
#endif
}