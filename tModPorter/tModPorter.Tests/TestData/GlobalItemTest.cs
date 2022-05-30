using Terraria;
using Terraria.ModLoader;

public class GlobalItemTest : GlobalItem
{
	public override bool NewPreReforge(Item item) { return false; /* comment */ }

	public override void GetWeaponKnockback(Item item, Player player, ref float knockback) { /* Empty */ }

	public override void GetWeaponCrit(Item item, Player player, ref int crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Item item, Player player, ref float add, ref float mult, ref float flat) { /* Empty */ }
}