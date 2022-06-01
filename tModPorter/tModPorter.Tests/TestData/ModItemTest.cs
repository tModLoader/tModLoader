using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class ModItemTest : ModItem
{
	public void IdentifierTest() {
		Console.Write(mod);
		item.SetDefaults(0);
		Console.Write(item);
		item.accessory = true;
		Console.Write(item.accessory);
		item.useTime += 2;
	}

	public override bool NewPreReforge() { return false; /* comment */ }

	public override bool UseItem(Player player) { return true; /* comment */ }

	public override void UseStyle(Player player) { /* comment */ }

	public override void NetRecieve(BinaryReader reader) { /* Empty */ }

	public override void GetWeaponKnockback(Player player, ref float knockback) { /* Empty */ }

	public override void GetWeaponCrit(Player player, ref int crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Player player, ref float add, ref float mult, ref float flat) { /* Empty */ }

	public override void Load(TagCompound tag) { /* Empty */ }

	public override TagCompound Save() => new TagCompound();
}