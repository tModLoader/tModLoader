using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class GlobalNPCTest : GlobalNPC
{
	public override bool PreKill(NPC npc) { return true; /* Empty */ }

	public override void OnKill(NPC npc) { /* Empty */ }

	public override bool SpecialOnKill(NPC npc) { return true; /* Empty */ }

	public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		spriteBatch.Draw(null, npc.Center - screenPos, drawColor);
		return true;
	}

	public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		spriteBatch.Draw(null, npc.Center - screenPos, drawColor);
	}
}
