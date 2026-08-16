using ExampleMod.Content.Biomes;
using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles
{
	public class ExampleBlock : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;

			DustType = ModContent.DustType<Sparkle>();
			VanillaFallbackOnModDeletion = TileID.DiamondGemspark;

			AddMapEntry(new Color(200, 200, 200));
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}

		public override void ChangeWaterfallStyle(ref int style) {
			style = ModContent.GetInstance<ExampleWaterfallStyle>().Slot;
		}

		public override void OnTileKilled(int i, int j) {
			// 10% chance to spawn a mouse when broken.
			if (Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(10)) {
				NPC.NewNPC(new EntitySource_TileBreak(i, j), i * 16, j * 16, NPCID.Mouse);
			}
		}
	}
}