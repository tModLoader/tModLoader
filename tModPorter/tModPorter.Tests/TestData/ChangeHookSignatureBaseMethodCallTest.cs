using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ChangeHookSignatureBaseMethodCallTest : ModProjectile
{
	public override bool PreDrawExtras() {
		return base.PreDrawExtras();
	}

	public override bool PreDraw(ref Color lightColor) {
		if (Main.rand.NextBool(3)) {
			lightColor = Color.Red;
			return base.PreDraw(ref lightColor);
		}
		else {
			var otherColor = Color.Blue;
			bool result = base.PreDraw(ref otherColor);
			return result;
		}
		bool test = base.PreDrawExtras();
		bool test2 = base.PreDraw(/* WillNotPreserve */ ref /* WillNotPreserve */ lightColor /* c */ );
		return base.PreDraw(ref lightColor);
	}

	public override void PostDraw(Color changedParameterName) {
		// These will all remain unchanged since the modder deviated from the expected parameter name, so we don't have enough information to map the arguments to the new signature.
		int test = changedParameterName.R;
		base.PostDraw(changedParameterName /*Comment*/);
		base.PostDraw(Main.rand.NextBool() ? Color.Red : changedParameterName);
	}
}

public class ChangeHookSignatureBaseMethodCallTest2 : ModItem
{
	public override bool OnPickup(Player player)
	{
		bool a = base.OnPickup(Main.LocalPlayer);
		bool b = base.OnPickup(Main.LocalPlayer /* B */);
		bool c = base.OnPickup(Main.rand.NextBool() ? Main.player[0] : new Player());
		bool d = base.OnPickup(player /* b */ ); // c
		return base.OnPickup(player);
	}
}