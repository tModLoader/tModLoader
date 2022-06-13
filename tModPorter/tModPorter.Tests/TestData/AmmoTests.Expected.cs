using Terraria;
using Terraria.ModLoader;

// TODO figure out how to accordingly port ref int damage -> ref StatModifier damage
public class AmmoModItemTest : ModItem
{
	public override void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) {
	}

#if COMPILE_ERROR
	public override void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) {
	}
#endif

	public override bool CanConsumeAmmo(Item ammo, Player player) { return true; /* Empty */ }

	public override void OnConsumeAmmo(Item ammo, Player player) { /* Empty */ }
}

public class AmmoGlobalItemTest : GlobalItem
{
	public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) {
	}

#if COMPILE_ERROR
	public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) {
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