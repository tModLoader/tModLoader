using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;

public class TexturesTest : Mod
{
	void Method() {
		Texture2D texture = null;

		var index = 0;
		texture = Main.projectileTexture[index];
		texture = Main.itemTexture[index];
		texture = Main.npcTexture[index];
		texture = Main.buffTexture[index];
		texture = Main.goreTexture[index];
		texture = Main.dustTexture[index];
		texture = Main.tileTexture[index];
		texture = Main.glowMaskTexture[index];
		texture = Main.npcHeadTexture[index];
		texture = Main.npcHeadBossTexture[index];

		texture = Main.chainsTexture[index];
		texture = Main.wireUITexture[index];

		texture = Main.wireTexture;
		texture = Main.wire2Texture;
		texture = Main.wire4Texture;

		texture = Main.chainTexture;
		texture = Main.chain2Texture;
		texture = Main.chain40Texture;

		texture = Main.inventoryBackTexture;
		texture = Main.inventoryBack2Texture;
		texture = Main.inventoryBack16Texture;

		texture = Main.blackTileTexture;
		texture = Main.magicPixel;
	}
}