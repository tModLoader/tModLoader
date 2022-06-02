using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

public class ShootModItemTest : ModItem
{
	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		float speedXLocal = velocity.X;
		float speedYLocal = velocity.Y;
		float knockBackLocal = knockback; // Only rename if parameter was knockBack

#if COMPILE_ERROR
		RefVector2DontPort(ref position /* Use ModifyShootStats for ref operations */);
		RefIntDontPort(ref type /* Use ModifyShootStats for ref operations */);
		RefIntDontPort(ref damage /* Use ModifyShootStats for ref operations */);
		RefFloatDontPort(ref velocity.X /* Use ModifyShootStats for ref operations */);
		RefFloatDontPort(ref velocity.Y /* Use ModifyShootStats for ref operations */);
		RefFloatDontPort(ref knockback /* Use ModifyShootStats for ref operations */);
#endif

		return true;
	}

	void RefIntDontPort(ref int x) { }
	void RefFloatDontPort(ref float x) { }
	void RefVector2DontPort(ref Vector2 v) { }

#if COMPILE_ERROR
	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float differentParameterName) {
		return true;
	}
#endif
}

public class ShootGlobalItemTest : GlobalItem
{
	public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		return true;
	}
}

public class ShootModPlayerTest : ModPlayer
{
	public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		return true;
	}
}