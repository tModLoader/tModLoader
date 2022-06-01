using System;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ModNPCTest : ModNPC
{
	public void IdentifierTest() {
		Console.Write(npc);
	}

	public override void NPCLoot() {  /*empty*/ }

	public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) { return true; }

	public override void PostDraw(SpriteBatch spriteBatch, Color drawColor) { /* Empty */ }
}
