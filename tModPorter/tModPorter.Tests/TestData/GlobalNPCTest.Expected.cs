﻿using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class GlobalNPCTest : GlobalNPC
{
	public override bool PreKill(NPC npc) { return true; /* Empty */ }

	public override void OnKill(NPC npc) { /* Empty */ }

	public override bool SpecialOnKill(NPC npc) { return true; /* Empty */ }

	public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		// not-yet-implemented
		spriteBatch.Draw(null, npc.Center - screenPos, drawColor);
		// instead-expect
		spriteBatch.Draw(null, npc.Center - Main.screenPosition, drawColor);
		return true;
	}

	public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		// not-yet-implemented
		spriteBatch.Draw(null, npc.Center - screenPos, drawColor);
		// instead-expect
		spriteBatch.Draw(null, npc.Center - Main.screenPosition, drawColor);
	}

#if COMPILE_ERROR
	public override bool CanHitNPC(NPC npc, NPC target)/* tModPorter Suggestion: Return true instead of null */ {
		return null;
	}
#endif

	public override void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment)/* tModPorter Note: bossLifeScale -> balance (bossAdjustment is different, see the docs for details) */
	{
	}
}