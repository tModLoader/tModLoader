using System;
using System.Collections.Generic;
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
		Console.Write(bossBag/* tModPorter Note: Removed. Spawn the treasure bag alongside other loot via npcLoot.Add(ItemDropRule.BossBag(type)) */);
#endif
	}

	public override bool PreKill() { return true; /*empty*/ }

	public override void OnKill() { /*empty*/ }

	public override bool SpecialOnKill() { return true; /* Empty */ }

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		Vector2 screen = screenPos - Vector2.One * 6f;
		return true;
	}

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		Vector2 screen = screenPos - Vector2.One * 6f;
	}

#if COMPILE_ERROR
	public override string[] AltTextures/* tModPorter Suggestion: Create a ITownNPCProfile, in its GetTextureNPCShouldUse, check for npc.altTexture to return the texture you want. Then, use TownNPCProfile hook to return an instance of that ITownNPCProfile */ => new string[0];

	public override List<string> SetNPCNameList()/* tModPorter Suggestion: Return a list of names */ { return "Name"; }
#endif
}
