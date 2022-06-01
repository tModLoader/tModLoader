using System;
using static tModPorter.Rewriters.RenameRewriter;

namespace tModPorter;

public static partial class Config
{
	private static void AddTextureRenames() {
		var getValue = AccessMember("Value");

		void RenameTextureAsset(string from, string to) =>	RenameStaticField("Terraria.Main", from, to, newType: "Terraria.GameContent.TextureAssets").FollowBy(getValue);
		void RenameFontAsset(string from, string to) =>		RenameStaticField("Terraria.Main", from, to, newType: "Terraria.GameContent.FontAssets").FollowBy(getValue);


		RenameFontAsset(from: "fontDeathText",	to: "DeathText");
		RenameFontAsset(from: "fontItemText",	to: "ItemStack");
		RenameFontAsset(from: "fontMouseText",	to: "MouseText");
		RenameFontAsset(from: "fontCombatText", to: "CombatText");

		RenameTextureAsset(from: "projectileTexture",	to: "Projectile");
		RenameTextureAsset(from: "itemTexture",			to: "Item");
		RenameTextureAsset(from: "npcTexture",			to: "Npc");
		RenameTextureAsset(from: "buffTexture",			to: "Buff");
		RenameTextureAsset(from: "goreTexture",			to: "Gore");
		RenameTextureAsset(from: "dustTexture",			to: "Dust");
		RenameTextureAsset(from: "tileTexture",			to: "Tile");
		RenameTextureAsset(from: "glowMaskTexture",		to: "GlowMask");
		RenameTextureAsset(from: "npcHeadTexture",		to: "NpcHead");
		RenameTextureAsset(from: "npcHeadBossTexture",	to: "NpcHeadBoss");
		RenameTextureAsset(from: "chainsTexture",		to: "Chains");
		RenameTextureAsset(from: "blackTileTexture",	to: "BlackTile");
		RenameTextureAsset(from: "magicPixel",			to: "MagicPixel");
		RenameTextureAsset(from: "wireUITexture",		to: "WireUi");

		void RenameMultipleTextures(int n, Func<string, string> from, Func<string, string> to) {
			for (int i = 0; i <= n; i++) {
				var s = i > 1 ? i.ToString() : "";
				RenameTextureAsset(from(s), to(s));
			}
		}

		RenameMultipleTextures(4,	s => $"wire{s}Texture",				s => $"Wire{s}");
		RenameMultipleTextures(43,	s => $"chain{s}Texture",			s => $"Chain{s}");
		RenameMultipleTextures(18,	s => $"inventoryBack{s}Texture",	s => $"InventoryBack{s}");
		RenameMultipleTextures(3,	s => $"sun{s}Texture",				s => $"Sun{s}");
	}
}

