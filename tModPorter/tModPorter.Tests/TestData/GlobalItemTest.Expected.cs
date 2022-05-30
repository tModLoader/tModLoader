using Terraria;
using Terraria.ModLoader;

public class GlobalItemTest : GlobalItem
{
	public override bool PreReforge(Item item) { return false; /* comment */ }

	public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback) { /* Empty */ }

	public override void ModifyWeaponCrit(Item item, Player player, ref float crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) { /* Empty */ }
}