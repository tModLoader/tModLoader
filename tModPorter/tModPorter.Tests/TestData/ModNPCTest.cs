using System;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ModNPCTest : ModNPC
{
	public void IdentifierTest() {
		Console.Write(npc);
		Console.Write(aiType);
		Console.Write(animationType);
		Console.Write(music);
		Console.Write(musicPriority);
		Console.Write(drawOffsetY);
		Console.Write(banner);
		Console.Write(bannerItem);

#if COMPILE_ERROR
		Console.Write(bossBag);
#endif
	}

	public override bool PreNPCLoot() { return true; /*empty*/ }

	public override void NPCLoot() { /*empty*/ }

	public override bool SpecialNPCLoot() { return true; /* Empty */ }

	public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) {
		Vector2 screen = Main.screenPosition - Vector2.One * 6f;
		return true;
	}

	public override void PostDraw(SpriteBatch spriteBatch, Color drawColor) {
		Vector2 screen = Main.screenPosition - Vector2.One * 6f;
	}

#if COMPILE_ERROR
	public override string[] AltTextures => new string[0];

	public override string TownNPCName() { return "Name"; }
#endif
}
