using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class GlobalNPCTest : GlobalNPC
{
	public override void NPCLoot(NPC npc) { /* Empty */ }

	public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) { return true; }

	public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) { /* Empty */ }
}
