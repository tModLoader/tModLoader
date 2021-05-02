using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
		private Color fishingLineColor; //TODO: Is this synchronized in multiplayer?

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
			Projectile.CloneDefaults(ProjectileID.BobberWooden);
			
			DrawOriginOffsetY = -8; // Adjusts the draw position
		}

		// What if we want to randomize the line color
		public override void AI() {
			if (!initialized) {
				// Decide color of the pole by getting a random entry from the color array
				fishingLineColor = Main.rand.Next(PossibleLineColors);
				initialized = true;
			}
			
			// Always ensure that graphics-related code doesn't run on dedicated servers via this check.
			if (!Main.dedServ) {
				// Create some light based on the color of the line.
				Lighting.AddLight(Projectile.Center, fishingLineColor.R / 255, fishingLineColor.G / 255, fishingLineColor.B / 255);
			}
		}

		public override bool PreDrawExtras() {
			// Change these two values in order to change the origin of where the line is being drawn
			const int XPositionAdditive = 45;
			const float YPositionAdditive = 35f;

			Player player = Main.player[Projectile.owner];
			
			if (!Projectile.bobber || player.inventory[player.selectedItem].holdStyle <= 0)
				return false;

			Vector2 lineOrigin = player.MountedCenter;
			lineOrigin.Y += player.gfxOffY;
			int type = player.inventory[player.selectedItem].type;

			// This variable is used to account for Gravitation Potions
			float gravity = player.gravDir;

			if (type == ModContent.ItemType<Items.Tools.ExampleFishingRod>()) {
				lineOrigin.X += XPositionAdditive * player.direction;

				if (player.direction < 0)
					lineOrigin.X -= 13f;
				
				lineOrigin.Y -= YPositionAdditive * gravity;
			}

			if (gravity == -1f)
				lineOrigin.Y -= 12f;

			// RotatedRelativePoint adjusts lineOrigin to account for player rotation.
			lineOrigin = player.RotatedRelativePoint(lineOrigin + new Vector2(8f), true) - new Vector2(8f);
			Vector2 playerToProjectile = Projectile.Center - lineOrigin;
			bool canDraw = true;

			if (playerToProjectile.X == 0f && playerToProjectile.Y == 0f)
				return false;

			float playerToProjectileMagnitude = playerToProjectile.Length();
			playerToProjectileMagnitude = 12f / playerToProjectileMagnitude;
			playerToProjectile *= playerToProjectileMagnitude;
			lineOrigin -= playerToProjectile;
			playerToProjectile = Projectile.Center - lineOrigin;

			// This math draws the line, while allowing the line to sag.
			while (canDraw) {
				float height = 12f;
				float positionMagnitude = playerToProjectile.Length();

				if (float.IsNaN(positionMagnitude) || float.IsNaN(positionMagnitude))
					break;

				if (positionMagnitude < 20f) {
					height = positionMagnitude - 8f;
					canDraw = false;
				}

				playerToProjectile *= 12f / positionMagnitude;
				lineOrigin += playerToProjectile;
				playerToProjectile.X = Projectile.position.X + Projectile.width * 0.5f - lineOrigin.X;
				playerToProjectile.Y = Projectile.position.Y + Projectile.height * 0.1f - lineOrigin.Y;

				if (positionMagnitude > 12f) {
					float positionInverseMultiplier = 0.3f;
					float absVelocitySum = Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y);

					absVelocitySum = Math.Min(absVelocitySum, 16f);
					absVelocitySum = 1f - absVelocitySum / 16f;
					positionInverseMultiplier *= absVelocitySum;
					absVelocitySum = positionMagnitude / 80f;
					absVelocitySum = Math.Min(absVelocitySum, 1f);

					positionInverseMultiplier *= absVelocitySum;
					positionInverseMultiplier = Math.Max(positionInverseMultiplier, 0f);

					absVelocitySum = 1f - Projectile.localAI[0] / 100f;
					positionInverseMultiplier *= absVelocitySum;

					if (playerToProjectile.Y > 0f) {
						playerToProjectile.Y *= 1f + positionInverseMultiplier;
						playerToProjectile.X *= 1f - positionInverseMultiplier;
					}
					else {
						absVelocitySum = Math.Abs(Projectile.velocity.X) / 3f;
						absVelocitySum = Math.Min(absVelocitySum, 1f);
						absVelocitySum -= 0.5f;
						positionInverseMultiplier *= absVelocitySum;

						if (positionInverseMultiplier > 0f) {
							positionInverseMultiplier *= 2f;
						}

						playerToProjectile.Y *= 1f + positionInverseMultiplier;
						playerToProjectile.X *= 1f - positionInverseMultiplier;
					}
				}

				var texture = TextureAssets.FishingLine.Value;
				var linePos = new Vector2(lineOrigin.X - Main.screenPosition.X + texture.Width * 0.5f, lineOrigin.Y - Main.screenPosition.Y + texture.Height * 0.5f);
				var sourceRectangle = new Rectangle(0, 0, texture.Width, (int)height);
				var lineColor = Lighting.GetColor(lineOrigin.ToTileCoordinates(), fishingLineColor); // This color decides the color of the fishing line. The color is randomized as decided in the AI.
				float rotation = playerToProjectile.ToRotation() - MathHelper.PiOver2;
				var origin = new Vector2(texture.Width * 0.5f, 0f);

				Main.EntitySpriteDraw(texture, linePos, sourceRectangle, lineColor, rotation, origin, 1f, SpriteEffects.None, 0);
			}
			
			return false;
		}
	}
}