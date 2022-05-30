using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class ModPlayerTest : ModPlayer
{
	public void IdentifierTest() {
		Console.Write(player);
	}

	public override void GetWeaponKnockback(Item item, ref float knockback) { /* Empty */ }

	public override void GetWeaponCrit(Item item, ref int crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Item item, ref float add, ref float mult, ref float flat) { /* Empty */ }

	public override void Load(TagCompound tag) { /* Empty */ }
}
