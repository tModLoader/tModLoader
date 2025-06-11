using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	// ExampleBobber is a fishing bobber spawned by ExampleFishingRod.
	// Aside from the code in SetDefaults, everything else should be ignored when making a typical bobber projectile.
	// Typically the fishing rod item decides the line color, but this bobber decides its own line color and serves as an example of using OnSpawn, SendExtraAI, and ReceiveExtraAI to sync a random value determined when spawned.
	public class ExampleBobber : ModProjectile
	{
		public static readonly Color[] PossibleLineColors = [
			new Color(255, 215, 0), // A gold color
			new Color(0, 191, 255) // A blue color
		];

		// This holds the index of the fishing line color in the PossibleLineColors array.
		private int fishingLineColorIndex;

		public Color FishingLineColor => PossibleLineColors[fishingLineColorIndex];

		public override void SetDefaults() {
			// These are copied through the CloneDefaults method
			// Projectile.width = 14;
			// Projectile.height = 14;
			// Projectile.aiStyle = 61;
			// Projectile.bobber = true;
			// Projectile.penetrate = -1;
			// Projectile.netImportant = true;
			Projectile.CloneDefaults(ProjectileID.BobberWooden);

			DrawOriginOffsetY = -8; // Adjusts the draw position
		}

		public override void OnSpawn(IEntitySource source) {
			// Decide color of the pole by getting the index of a random entry from the PossibleLineColors array.
			fishingLineColorIndex = (byte)Main.rand.Next(PossibleLineColors.Length);
		}

		public override void AI() {
			// Always ensure that graphics-related code doesn't run on dedicated servers via this check.
			if (!Main.dedServ) {
				// Create some light based on the color of the line.
				Lighting.AddLight(Projectile.Center, FishingLineColor.ToVector3());
			}
		}

		// These last two methods are required so the line color is properly synced in multiplayer.
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((byte)fishingLineColorIndex);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			fishingLineColorIndex = reader.ReadByte();
		}
	}
}