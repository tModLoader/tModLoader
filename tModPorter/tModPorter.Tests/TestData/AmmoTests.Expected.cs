using Terraria;
using Terraria.ModLoader;

public class AmmoModItemTest : ModItem
{
	public override void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref StatModifier weaponDamage, ref StatModifier ammoDamage, ref float knockback)/* tModPorter Note: Weapon damage is no longer included in ammoDamage.Flat. Use weaponDamage instead. */ {
		ammoDamage *= 1.1f;
	}
}

public class AmmoGlobalItemTest : GlobalItem
{
	public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier weaponDamage, ref StatModifier ammoDamage, ref float knockback)/* tModPorter Note: Weapon damage is no longer included in ammoDamage.Flat. Use weaponDamage instead. */ {
		ammoDamage *= 1.1f;
	}
}

public class AmmoModItemTest143 : ModItem
{
	public override void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref StatModifier weaponDamage, ref StatModifier ammoDamage, ref float knockback)/* tModPorter Note: Weapon damage is no longer included in ammoDamage.Flat. Use weaponDamage instead. */ {
	}

#if COMPILE_ERROR
	public override void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref StatModifier weaponDamage, ref StatModifier ammoDamage, ref float knockback)/* tModPorter Note: Weapon damage is no longer included in ammoDamage.Flat. Use weaponDamage instead. */ {
	}
#endif

	public override bool CanConsumeAmmo(Item ammo, Player player) { return true; /* Empty */ }

	public override void OnConsumeAmmo(Item ammo, Player player) { /* Empty */ }
}

public class AmmoGlobalItemTest143 : GlobalItem
{
	public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier weaponDamage, ref StatModifier ammoDamage, ref float knockback)/* tModPorter Note: Weapon damage is no longer included in ammoDamage.Flat. Use weaponDamage instead. */ {
	}

#if COMPILE_ERROR
	public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier weaponDamage, ref StatModifier ammoDamage, ref float knockback)/* tModPorter Note: Weapon damage is no longer included in ammoDamage.Flat. Use weaponDamage instead. */ {
	}
#endif

	// item -> weapon
	public override bool CanConsumeAmmo(Item weapon, Item ammo, Player player) { return true; /* Empty */ }

	// item -> weapon
	public override void OnConsumeAmmo(Item weapon, Item ammo, Player player) { /* Empty */ }
}

public class AmmoModPlayerItemTest : ModPlayer
{
	public override bool CanConsumeAmmo(Item weapon, Item ammo) { return true; /* Empty */ }
}