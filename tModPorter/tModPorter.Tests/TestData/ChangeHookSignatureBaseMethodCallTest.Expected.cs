using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ChangeHookSignatureBaseMethodCallTest : ModProjectile
{
	public override bool PreDrawExtras(Player player)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
		return base.PreDrawExtras(player);
	}

	public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
		if (Main.rand.NextBool(3)) {
			lightColor = Color.Red;
			return base.PreDraw(player, ref lightColor);
		}
		else {
			var otherColor = Color.Blue;
			bool result = base.PreDraw(player, ref otherColor);
			return result;
		}
#if COMPILE_ERROR
		bool test = base.PreDrawExtras();
#endif
		bool test2 = base.PreDraw(player, ref lightColor /* c */ );
		return base.PreDraw(player, ref lightColor);
	}

	public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
		// These will all remain unchanged since the modder deviated from the expected parameter name, so we don't have enough information to map the arguments to the new signature.
#if COMPILE_ERROR
		int test = changedParameterName.R;
		base.PostDraw(changedParameterName /*Comment*/);
		base.PostDraw(Main.rand.NextBool() ? Color.Red : changedParameterName);
#endif
	}
}

public class ChangeHookSignatureBaseMethodCallTest2 : ModItem
{
	public override bool OnPickup(WorldItem item, Player player)
	{
		bool a = base.OnPickup(item, Main.LocalPlayer);
		bool b = base.OnPickup(item, Main.LocalPlayer /* B */);
		bool c = base.OnPickup(item, Main.rand.NextBool() ? Main.player[0] : new Player());
		bool d = base.OnPickup(item, player /* b */ ); // c
		return base.OnPickup(item, player);
	}
}