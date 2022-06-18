using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;

public class TexturesTest : Mod
{
	void Method() {
		Texture2D texture = null;

		var index = 0;
		texture = TextureAssets.Projectile[index].Value;
		texture = TextureAssets.Item[index].Value;
		texture = TextureAssets.Npc[index].Value;
		texture = TextureAssets.Buff[index].Value;
		texture = TextureAssets.Gore[index].Value;
		texture = TextureAssets.Tile[index].Value;
		texture = TextureAssets.GlowMask[index].Value;
		texture = TextureAssets.NpcHead[index].Value;
		texture = TextureAssets.NpcHeadBoss[index].Value;

		texture = TextureAssets.Chains[index].Value;
		texture = TextureAssets.WireUi[index].Value;
		texture = TextureAssets.Gem[index].Value;

		texture = TextureAssets.Dust.Value;
		texture = TextureAssets.Sun.Value;
		texture = TextureAssets.Sun2.Value;
		texture = TextureAssets.Sun3.Value;

		texture = TextureAssets.Wire.Value;
		texture = TextureAssets.Wire2.Value;
		texture = TextureAssets.Wire4.Value;

		texture = TextureAssets.Chain.Value;
		texture = TextureAssets.Chain2.Value;
		texture = TextureAssets.Chain40.Value;

		texture = TextureAssets.InventoryBack.Value;
		texture = TextureAssets.InventoryBack2.Value;
		texture = TextureAssets.InventoryBack16.Value;

		texture = TextureAssets.BlackTile.Value;
		texture = TextureAssets.MagicPixel.Value;
		texture = TextureAssets.FishingLine.Value;
	}
}