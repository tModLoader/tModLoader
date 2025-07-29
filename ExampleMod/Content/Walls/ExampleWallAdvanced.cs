using ExampleMod.Common.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Walls
{
	// This is a more advanced ModWall showing off animation, dynamic dust, light emitting, and simple custom framing logic
	public class ExampleWallAdvanced : ModWall
	{
		public override void SetStaticDefaults() {
			Main.wallHouse[Type] = true;
			Main.wallBlend[Type] = WallID.CogWall; // Prevent black line being drawn between this and CogWall

			DustType = DustID.Stone;

			AddMapEntry(new Color(68, 68, 68));
		}

		public override bool CreateDust(int i, int j, ref int type) {
			type = DustType;
			if (Main.tile[i, j].WallFrameNumber == 0)
				type = DustID.GemEmerald;
			return true;
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 3 : 10;
		}

		public override void AnimateWall(ref byte frame, ref byte frameCounter) {
			// Loop through 2 frames of animation, changing every 5 game frames
			if (++frameCounter >= 5) {
				frameCounter = 0;
				frame = (byte)(++frame % 2);
			}
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (!Main.dayTime) {
				r = 0.1f;
				g = 0.5f;
				b = 0f;
			}
		}

		public override bool WallFrame(int i, int j, bool randomizeFrame, ref int style, ref int frameNumber) {
			if (randomizeFrame) {
				// Here we make the chance of WallFrameNumber 0 very rare, just for visual variety: https://i.imgur.com/9Irak3p.png
				if (frameNumber == 0 && WorldGen.genRand.NextBool(3, 4)) {
					frameNumber = WorldGen.genRand.Next(1, 3);
				}
			}
			return base.WallFrame(i, j, randomizeFrame, ref style, ref frameNumber);
		}

		public override bool CanBeTeleportedTo(int i, int j, Player player, string context) {
			// Limit teleportation similar to how teleporting to a location with Dungeon walls or Lihzahrd walls is limited by defeating some bosses.
			return DownedBossSystem.downedMinionBoss;
		}
	}
}