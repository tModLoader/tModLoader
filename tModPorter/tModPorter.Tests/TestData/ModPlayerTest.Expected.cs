using System;
using Terraria;
using Terraria.ModLoader;

public class ModPlayerTest : ModPlayer
{
	public void IdentifierTest() {
		Console.Write(Player);
	}

	public override void ModifyWeaponKnockback(Item item, ref StatModifier knockback) { /* Empty */ }

	public override void ModifyWeaponCrit(Item item, ref float crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Item item, ref StatModifier damage) { /* Empty */ }
}
