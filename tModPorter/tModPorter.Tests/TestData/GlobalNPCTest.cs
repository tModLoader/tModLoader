using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class GlobalNPCTest : GlobalNPC
{
	public override bool PreNPCLoot(NPC npc) { return true; /* Empty */ }

	public override void NPCLoot(NPC npc) { /* Empty */ }

	public override bool SpecialNPCLoot(NPC npc) { return true; /* Empty */ }

	public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
		spriteBatch.Draw(null, npc.Center - Main.screenPosition, drawColor);
		return true;
	}

	public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
		spriteBatch.Draw(null, npc.Center - Main.screenPosition, drawColor);
	}
}
