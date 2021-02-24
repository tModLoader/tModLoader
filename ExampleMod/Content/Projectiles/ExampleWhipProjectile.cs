using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	public class ExampleWhipProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.IsAWhip[Projectile.type] = true; // This makes the projectile use whip collision detection and allows flasks to be applied to it.
		}

		public override void SetDefaults() {
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ownerHitCheck = true; // This prevents the projectile from hitting through solid tiles.
			Projectile.extraUpdates = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		private float Timer {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		private float ChargeTime {
			get => Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Without PiOver2, the rotation would be off by 90 degrees counterclockwise.

			Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;
			Projectile.spriteDirection = Vector2.Dot(Projectile.velocity, Vector2.UnitX) >= 0f ? 1 : -1;

			// This statement handles charging. The maximum charge time is doubled to accommodate for the projectile's extra update. If you don't want this functionality, replace the statement with just Timer++;
			if (owner.channel && ChargeTime < 120) {
				ChargeTime++;
			}
			else {
				Timer++;
			}

			Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);

			if (Timer >= timeToFlyOut || owner.itemAnimation <= 0) {
				Projectile.Kill();
				return;
			}

			owner.heldProj = Projectile.whoAmI;

			// These two lines ensure that the timing of the player's use animation is correct.
			owner.itemAnimation = owner.itemAnimationMax - (int)(Timer / Projectile.MaxUpdates);
			owner.itemTime = owner.itemAnimation;

			if (Timer == timeToFlyOut / 2) {
				// Plays a whipcrack sound at the tip of the whip.
				List<Vector2> temp = new List<Vector2>();
				Projectile.FillWhipControlPoints(Projectile, temp);
				SoundEngine.PlaySound(SoundID.Item153, temp[temp.Count - 1]);
			}
		}

		public override void GetWhipSettings(Player player, ref float timeToFlyOut, ref int segments, ref float rangeMultiplier) {
			segments = 20 + (int)ChargeTime / 12;
			rangeMultiplier = 1 + (int)ChargeTime / 120f;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
		}

		// This method draws a line between all points of the whip, in case there's empty space between the sprites.
		private void DrawLine(SpriteBatch spriteBatch, List<Vector2> list) {
			Texture2D texture = TextureAssets.FishingLine.Value;
			Rectangle frame = texture.Frame();
			Vector2 origin = new Vector2(frame.Width / 2, 2);

			Vector2 pos = list[0];
			for (int i = 0; i < list.Count - 1; i++) {
				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;

				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.White);
				Vector2 scale = new Vector2(1f, (diff.Length() + 2) / frame.Height);

				spriteBatch.Draw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0f);

				pos += diff;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			List<Vector2> list = new List<Vector2>();
			Projectile.FillWhipControlPoints(Projectile, list);

			DrawLine(spriteBatch, list);

			SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 pos = list[0];
			for (int i = 0; i < list.Count - 1; i++) {
				Rectangle frame = new Rectangle(0, 0, 10, 26);
				Vector2 origin = new Vector2(5, 8);

				if (i == list.Count - 2) {
					frame.Y = 74;
					frame.Height = 18;
				}
				else if (i > 10) {
					frame.Y = 58;
					frame.Height = 16;
				}
				else if (i > 5) {
					frame.Y = 42;
					frame.Height = 16;
				}
				else if (i > 0) {
					frame.Y = 26;
					frame.Height = 16;
				}

				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;

				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Color color = Lighting.GetColor(element.ToTileCoordinates());

				spriteBatch.Draw(texture, pos - Main.screenPosition, frame, color, rotation, origin, 1, flip, 0);

				pos += diff;
			}
			return false;
		}
	}
}
