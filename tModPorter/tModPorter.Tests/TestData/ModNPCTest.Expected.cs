using System;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ModNPCTest : ModNPC
{
	public void IdentifierTest() {
		Console.Write(NPC);
	}

	public override void OnKill() {  /*empty*/ }

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { return true; }

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		Vector2 screen = screenPos - Vector2.One * 6f;
	}
}
