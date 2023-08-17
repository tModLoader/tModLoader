using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

public class ShootModItemTest : ModItem
{
	public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
		float speedXLocal = speedX;
		float speedYLocal = speedY;
		float knockBackLocal = knockBack;

		RefVector2DontPort(ref position);
		RefIntDontPort(ref type);
		RefIntDontPort(ref damage);
		RefFloatDontPort(ref speedX);
		RefFloatDontPort(ref speedY);
		RefFloatDontPort(ref knockBack);

		return true;
	}

	void RefIntDontPort(ref int x) { }
	void RefFloatDontPort(ref float x) { }
	void RefVector2DontPort(ref Vector2 v) { }
}

public class ShootGlobalItemTest : GlobalItem
{
	public override bool Shoot(Item item, Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
		return true;
	}
}

public class ShootModPlayerTest : ModPlayer
{
	public override bool Shoot(Item item, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
		return true;
	}
}