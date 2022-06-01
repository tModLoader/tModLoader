﻿using System;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ModProjectileTest : ModProjectile
{
	public void IdentifierTest() {
		Console.Write(projectile);
	}

	public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) { return true; }

	public override bool PreDrawExtras(SpriteBatch spriteBatch) { return true; }

	public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) { return true; }

	public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) { /* Empty */ }
}
