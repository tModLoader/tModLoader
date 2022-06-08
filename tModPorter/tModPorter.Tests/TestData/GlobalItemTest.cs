using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class GlobalItemTest : GlobalItem
{
	public override bool UseItem(Item item, Player player) => false;

	public override bool NewPreReforge(Item item) { return false; /* comment */ }

	public override void HoldStyle(Item item, Player player) { /* comment */ }

	public override void UseStyle(Item item, Player player) { /* comment */ }

	public override bool CanEquipAccessory(Item item, Player player, int slot) { return true; /* comment */ }

	public override void GetWeaponKnockback(Item item, Player player, ref float knockback) { /* Empty */ }

	public override void GetWeaponCrit(Item item, Player player, ref int crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Item item, Player player, ref float add, ref float mult, ref float flat) {
		add += 0.1f;
		mult *= 0.2f;
		flat += 4;
	}

	public override void Load(Item item, TagCompound tag) { /* Empty */ }

#if COMPILE_ERROR
	public override TagCompound Save(Item item) => new TagCompound();
#endif
}