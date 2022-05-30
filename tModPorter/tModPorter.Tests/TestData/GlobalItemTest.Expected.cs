using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;

public class GlobalItemTest : GlobalItem
{
	public override bool PreReforge(Item item) { return false; /* comment */ }

	public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) { /* comment */ }

	public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback) { /* Empty */ }

	public override void ModifyWeaponCrit(Item item, Player player, ref float crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) { /* Empty */ }

	public override void LoadData(Item item, TagCompound tag) { /* Empty */ }
}