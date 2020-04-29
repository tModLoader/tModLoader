using ExampleMod.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Projectiles
{
	public class ExampleBobber : ModProjectile
	{
		// You can use vanilla textures by using the format: Terraria/Projectile_<ID>
		public override string Texture => "Terraria/Projectile_" + ProjectileID.BobberWooden;

		private bool initialized = false;
		private Color fishingLineColor;
		public Color[] PossibleLineColors = new Color[]
		{
			new Color(255, 215, 0), //a gold color
			new Color(0, 191, 255) // a blue color
		};

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Bobber");
		}

		public override void SetDefaults() {
			//These are copied through the CloneDefaults method
			//projectile.width = 14;
			//projectile.height = 14;
			//projectile.aiStyle = 61;
			//projectile.bobber = true;
			//projectile.penetrate = -1;
			projectile.CloneDefaults(ProjectileID.BobberWooden);
		}

		//What if we want to randomize the line color
		public override void AI() {
			if (!initialized) {
				//Decide color of the pole by randomizing the array
				fishingLineColor = Main.rand.Next(PossibleLineColors);
				initialized = true;
			}
		}

		public override bool PreDrawExtras(SpriteBatch spriteBatch) {
			//Create some light based on the color of the line; this could also be in the AI function
			Lighting.AddLight(projectile.Center, fishingLineColor.R / 255, fishingLineColor.G / 255, fishingLineColor.B / 255);

			//Change these two values in order to change the origin of where the line is being drawn
			int xPositionAdditive = 45;
			float yPositionAdditive = 35f;

			Player player = Main.player[projectile.owner];
			if (!projectile.bobber || player.inventory[player.selectedItem].holdStyle <= 0)
				return false;

			float originX = player.MountedCenter.X;
			float originY = player.MountedCenter.Y;
			originY += player.gfxOffY;
			int type = player.inventory[player.selectedItem].type;
			//This variable is used to account for Gravitation Potions
			float gravity = player.gravDir;

			if (type == ItemType<ExampleFishingRod>()) {
				originX += (float)(xPositionAdditive * player.direction);
				if (player.direction < 0) {
					originX -= 13f;
				}
				originY -= yPositionAdditive * gravity;
			}

			if (gravity == -1f) {
				originY -= 12f;
			}
			Vector2 mountedCenter = new Vector2(originX, originY);
			mountedCenter = player.RotatedRelativePoint(mountedCenter + new Vector2(8f), true) - new Vector2(8f);
			Vector2 lineOrigin = projectile.Center - mountedCenter;
			bool canDraw = true;
			if (lineOrigin.X == 0f && lineOrigin.Y == 0f)
				return false;

			float projPosMagnitude = lineOrigin.Length();
			projPosMagnitude = 12f / projPosMagnitude;
			lineOrigin.X *= projPosMagnitude;
			lineOrigin.Y *= projPosMagnitude;
			mountedCenter -= lineOrigin;
			lineOrigin = projectile.Center - mountedCenter;

			while (canDraw) {
				float height = 12f;
				float positionMagnitude = lineOrigin.Length();
				if (float.IsNaN(positionMagnitude) || float.IsNaN(positionMagnitude))
					break;

				if (positionMagnitude < 20f) {
					height = positionMagnitude - 8f;
					canDraw = false;
				}
				positionMagnitude = 12f / positionMagnitude;
				lineOrigin.X *= positionMagnitude;
				lineOrigin.Y *= positionMagnitude;
				mountedCenter += lineOrigin;
				lineOrigin = projectile.Center - mountedCenter;
				if (positionMagnitude > 12f) {
					float positionInverseMultiplier = 0.3f;
					float absVelocitySum = Math.Abs(projectile.velocity.X) + Math.Abs(projectile.velocity.Y);
					if (absVelocitySum > 16f) {
						absVelocitySum = 16f;
					}
					absVelocitySum = 1f - absVelocitySum / 16f;
					positionInverseMultiplier *= absVelocitySum;
					absVelocitySum = positionMagnitude / 80f;
					if (absVelocitySum > 1f) {
						absVelocitySum = 1f;
					}
					positionInverseMultiplier *= absVelocitySum;
					if (positionInverseMultiplier < 0f) {
						positionInverseMultiplier = 0f;
					}
					absVelocitySum = 1f - projectile.localAI[0] / 100f;
					positionInverseMultiplier *= absVelocitySum;
					if (lineOrigin.Y > 0f) {
						lineOrigin.Y *= 1f + positionInverseMultiplier;
						lineOrigin.X *= 1f - positionInverseMultiplier;
					}
					else {
						absVelocitySum = Math.Abs(projectile.velocity.X) / 3f;
						if (absVelocitySum > 1f) {
							absVelocitySum = 1f;
						}
						absVelocitySum -= 0.5f;
						positionInverseMultiplier *= absVelocitySum;
						if (positionInverseMultiplier > 0f) {
							positionInverseMultiplier *= 2f;
						}
						lineOrigin.Y *= 1f + positionInverseMultiplier;
						lineOrigin.X *= 1f - positionInverseMultiplier;
					}
				}
				//This color decides the color of the fishing line. The color is randomized as decided in the AI.
				Color lineColor = Lighting.GetColor((int)mountedCenter.X / 16, (int)(mountedCenter.Y / 16f), fishingLineColor);
				float rotation = (float)Math.Atan2((double)lineOrigin.Y, (double)lineOrigin.X) - MathHelper.PiOver2;

				Main.spriteBatch.Draw(Main.fishingLineTexture, new Vector2(mountedCenter.X - Main.screenPosition.X + (float)Main.fishingLineTexture.Width * 0.5f, mountedCenter.Y - Main.screenPosition.Y + (float)Main.fishingLineTexture.Height * 0.5f), new Rectangle?(new Rectangle(0, 0, Main.fishingLineTexture.Width, (int)height)), lineColor, rotation, new Vector2((float)Main.fishingLineTexture.Width * 0.5f, 0f), 1f, SpriteEffects.None, 0f);
			}
			return false;
		}
	}
}
