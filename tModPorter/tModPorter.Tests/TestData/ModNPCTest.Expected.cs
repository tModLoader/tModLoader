using System;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ModNPCTest : ModNPC
{
	public void IdentifierTest() {
		Console.Write(NPC);
		Console.Write(AIType);
		Console.Write(AnimationType);
		Console.Write(Music);
		Console.Write(SceneEffectPriority);
		Console.Write(DrawOffsetY);
		Console.Write(Banner);
		Console.Write(BannerItem);

#if COMPILE_ERROR
		Console.Write(bossBag /* Suggestion: Removed. Spawn the treasure bag alongside other loot via npcLoot.Add(ItemDropRule.BossBag(type)) */ );
#endif
	}

	public override bool PreKill() { return true; /*empty*/ }

	public override void OnKill() { /*empty*/ }

	public override bool SpecialOnKill() { return true; /* Empty */ }

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { return true; }

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		Vector2 screen = screenPos - Vector2.One * 6f;
	}
}
