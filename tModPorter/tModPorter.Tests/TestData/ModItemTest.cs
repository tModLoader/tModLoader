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

#if COMPILE_ERROR
	public override bool IgnoreDamageModifiers => false;

	public override bool OnlyShootOnSwing => false;
#endif

	public override bool CloneNewInstances => false;

	public override ModItem Clone() { return null; }

	public override bool NewPreReforge() { return false; /* comment */ }

	public override bool UseItem(Player player) { return true; /* comment */ }

	public override void HoldStyle(Player player) { /* comment */ }

	public override void UseStyle(Player player) { /* comment */ }

	public override void UpdateVanity(Player player, EquipType type) { /* comment */ }

	public override void NetRecieve(BinaryReader reader) { /* Empty */ }

	public override void GetWeaponKnockback(Player player, ref float knockback) { /* Empty */ }

	public override void GetWeaponCrit(Player player, ref int crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Player player, ref float add, ref float mult, ref float flat) {
		add += 0.1f;
		mult *= 0.2f;
		flat += 4;
	}

	public override void Load(TagCompound tag) { /* Empty */ }

#if COMPILE_ERROR
	public override TagCompound Save() => new TagCompound();
#endif
}