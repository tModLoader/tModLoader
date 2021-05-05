using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	public class ExampleBobber : ModProjectile
	{
		public static readonly Color[] PossibleLineColors = new Color[] {
			new Color(255, 215, 0), // A gold color
			new Color(0, 191, 255) // A blue color
		};

		private bool initialized;
		// This holds the index of the fishing line color in the PossibleLineColors array.
		private int fishingLineColorIndex;

		private Color FishingLineColor => PossibleLineColors[fishingLineColorIndex];

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Bobber");
		}

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

		// What if we want to randomize the line color
		public override void AI() {
			if (!initialized) {
				initialized = true;
				// Decide color of the pole by getting the index of a random entry from the PossibleLineColors array.
				fishingLineColorIndex = (byte)Main.rand.Next(PossibleLineColors.Length);
				// Tell the game to sync this projectile in multiplayer, so the line color is synced.
				Projectile.netUpdate = true;
			}

			// Always ensure that graphics-related code doesn't run on dedicated servers via this check.
			if (!Main.dedServ) {
				// Create some light based on the color of the line.
				Lighting.AddLight(Projectile.Center, FishingLineColor.ToVector3());
			}
		}

		public override void ModifyFishingLine(ref Vector2 lineOriginOffset, ref Color lineColor) {
			// Change these two values in order to change the origin of where the line is being drawn.
			// This will make it draw 47 pixels right and 31 pixels up from the player's center, while they are looking right and in normal gravity.
			lineOriginOffset = new Vector2(47, -31);
			// Sets the fishing line's color. Note that this will be overridden by the colored string accessories.
			lineColor = FishingLineColor;
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