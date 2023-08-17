using Terraria;
using Terraria.ModLoader;

// TODO figure out how to accordingly port ref int damage -> ref StatModifier damage
public class AmmoModItemTest : ModItem
{
	public override void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref int damage, ref float knockback) {
	}

	public override void PickAmmo(Player player, ref int type, ref float speed, ref int damage, ref float knockback) {
	}

	public override bool ConsumeAmmo(Player player) { return true; /* Empty */ }

	public override void OnConsumeAmmo(Player player) { /* Empty */ }
}

public class AmmoGlobalItemTest : GlobalItem
{
	public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref int damage, ref float knockback) {
	}

	public override void PickAmmo(Item item, Player player, ref int type, ref float speed, ref int damage, ref float knockback) {
	}

	// item -> weapon
	public override bool ConsumeAmmo(Item item, Player player) { return true; /* Empty */ }

	// item -> weapon
	public override void OnConsumeAmmo(Item item, Player player) { /* Empty */ }
}

public class AmmoModPlayerItemTest : ModPlayer
{
	public override bool ConsumeAmmo(Item weapon, Item ammo) { return true; /* Empty */ }
}