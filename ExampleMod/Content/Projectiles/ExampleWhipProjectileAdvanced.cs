using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	public class ExampleWhipProjectileAdvanced : ModProjectile
	{
		public override void SetStaticDefaults() {
			// This makes the projectile use whip collision detection and allows flasks to be applied to it.
			ProjectileID.Sets.IsAWhip[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.drawLayer = ProjectileDrawLayerID.HeldProj;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ownerHitCheck = true; // This prevents the projectile from hitting through solid tiles.
			Projectile.extraUpdates = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.DamageType = DamageClass.SummonMeleeSpeed;
			Projectile.WhipSettings.Segments = 10;
			Projectile.WhipSettings.RangeMultiplier = 1.5f;
		}

		private float Timer {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		// Projectile.ai[1] is used for the direction the swing will go in and is assigned when the projectile is spawned in.

		private float ChargeTime {
			get => Projectile.ai[2];
			set => Projectile.ai[2] = value;
		}

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Without PiOver2, the rotation would be off by 90 degrees counterclockwise.

			Projectile.Center = Main.GetPlayerArmPosition(Projectile, owner) + Projectile.velocity * Timer;
			// Vanilla uses Vector2.Dot(Projectile.velocity, Vector2.UnitX) here. Dot Product returns the difference between two vectors, 0 meaning they are perpendicular.
			// However, the use of UnitX basically turns it into a more complicated way of checking if the projectile's velocity is above or equal to zero on the X axis.
			Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

			// remove these 3 lines if you don't want the charging mechanic
			if (!Charge(owner)) {
				return; // timer doesn't update while charging, freezing the animation at the start.
			}

			Timer++;

			Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out _, out _);
			if (Timer >= timeToFlyOut || owner.itemAnimation <= 0) {
				Projectile.Kill();
				return;
			}

			owner.heldProj = Projectile.whoAmI;
			owner.MatchItemTimeToItemAnimation();
			if (Timer == timeToFlyOut / 2) {
				// Plays a whipcrack sound at the tip of the whip.
				List<Vector2> points = Projectile.WhipPointsForCollision;
				Projectile.FillWhipControlPoints(Projectile, points);
				SoundEngine.PlaySound(SoundID.Item153, points[points.Count - 1]);
			}

			// Spawn Dust along the whip path
			// This is the dust code used by Durendal. Consult the Terraria source code for even more examples, found in Projectile.AI_165_Whip.
			float swingProgress = Timer / timeToFlyOut;
			// This code limits dust to only spawn during the the actual swing.
			if (Utils.GetLerpValue(0.1f, 0.7f, swingProgress, clamped: true) * Utils.GetLerpValue(0.9f, 0.7f, swingProgress, clamped: true) > 0.5f && !Main.rand.NextBool(3)) {
				List<Vector2> points = Projectile.WhipPointsForCollision;
				points.Clear();
				Projectile.FillWhipControlPoints(Projectile, points);
				int pointIndex = Main.rand.Next(points.Count - 10, points.Count);
				Rectangle spawnArea = Utils.CenteredRectangle(points[pointIndex], new Vector2(30f, 30f));
				int dustType = DustID.Enchanted_Gold;
				if (Main.rand.NextBool(3))
					dustType = DustID.TintableDustLighted;

				// After choosing a randomized dust and a whip segment to spawn from, dust is spawned.
				Dust dust = Dust.NewDustDirect(spawnArea.TopLeft(), spawnArea.Width, spawnArea.Height, dustType, 0f, 0f, 100, Color.White);
				dust.position = points[pointIndex];
				dust.fadeIn = 0.3f;
				Vector2 spinningPoint = points[pointIndex] - points[pointIndex - 1];
				dust.noGravity = true;
				dust.velocity *= 0.5f;
				// This math causes these dust to spawn with a velocity perpendicular to the direction of the whip segments, giving the impression of the dust flying off like sparks.
				dust.velocity += spinningPoint.RotatedBy(owner.direction * ((float)Math.PI / 2f));
				dust.velocity *= 0.5f;
			}
		}

		// This method handles a charging mechanic.
		// If you remove this, also remove Item.channel = true from the item's SetDefaults.
		// Returns true if fully charged
		private bool Charge(Player owner) {
			// Like other whips, this whip updates twice per frame (Projectile.extraUpdates = 1), so 120 is equal to 1 second.
			if (!owner.channel || ChargeTime >= 120) {
				return true; // finished charging
			}

			ChargeTime++;

			if (ChargeTime % 12 == 0) // 1 segment per 12 ticks of charge.
				Projectile.WhipSettings.Segments++;

			// Increase range up to 2x for full charge.
			Projectile.WhipSettings.RangeMultiplier += 1 / 120f;

			// Reset the animation and item timer while charging.
			owner.itemAnimation = owner.itemAnimationMax;
			owner.itemTime = owner.itemTimeMax;

			return false; // still charging
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI; // Apply the targeting focus on the NPC who was hit.
			Projectile.damage = (int)(Projectile.damage * 0.7f); // Multihit penalty. Decrease the damage the more enemies the whip hits.

			// This is needed in order for OnProcHit in the WhipTagEffect to activate.
			if (Projectile.localAI[0] == 0f) {
				Projectile.localAI[0] = 1f;
				Main.player[Projectile.owner].TagEffectState.TryEnableProcOnNPC(Projectile.tagEffectType, target);
			}
		}

		// This method draws a line between all points of the whip, in case there's empty space between the sprites.
		private void DrawLine(List<Vector2> list) {
			Texture2D texture = TextureAssets.FishingLine.Value;
			Rectangle frame = texture.Frame();
			Vector2 origin = new Vector2(frame.Width / 2, 2);

			Vector2 pos = list[0];
			// This whip has a long range and this line is poking out of the front, so we will use list.Count - 2 instead of list.Count - 1.
			for (int i = 0; i < list.Count - 2; i++) { 
				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;

				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.White);
				Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

				pos += diff;
			}
		}

		public override bool PreDraw(Player player, ref Color lightColor) {
			List<Vector2> list = new List<Vector2>();
			Projectile.FillWhipControlPoints(Projectile, list);

			DrawLine(list);

			//Main.DrawWhip_WhipBland(Projectile, list);
			// The code below is for custom drawing.
			// If you don't want that, you can remove it all and instead call one of vanilla's DrawWhip methods, like above.
			// However, you must adhere to how they draw if you do.

			// This custom drawing allows us to specifically define which coordinates on the sprite are each segment.
			// That is why the sprite for ExampleWhipProjectileAdvanced doesn't have any padding.
			// For a more traditional drawing method, see ExampleWhipProjectile.

			SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			int totalSegments = Projectile.WhipSettings.Segments; // The number of segments this whip has.

			Texture2D texture = TextureAssets.Projectile[Type].Value;

			Vector2 pos = list[0];

			for (int i = 0; i < list.Count - 1; i++) {
				// These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
				// You can change them if they don't!
				Rectangle frame = new Rectangle(0, 0, 10, 26); // The size of the Handle (measured in pixels)
				Vector2 origin = new Vector2(5, 8); // Offset for where the player's hand will start measured from the top left of the image.
				float scale = 1;

				// These statements determine what part of the spritesheet to draw for the current segment.
				// They can also be changed to suit your sprite.
				if (i == list.Count - 2) {
					// This is the head of the whip. You need to measure the sprite to figure out these values.
					frame.Y = 74; // Distance from the top of the sprite to the start of the frame.
					frame.Height = 18; // Height of the frame.

					// For a more impactful look, this scales the tip of the whip up when fully extended, and down when curled up.
					Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
					float t = Timer / timeToFlyOut;
					scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
				}
				// Divide the middle of the whip (after the handle and before the head) by approximately 3 and use the middle segments in each third.
				// ExampleWhipProjectileAdvanced has 10 segments, so the following will result in 1 handle, 3 segment 1s, 3 segment 2s, 2 segment 3s, and 1 head.
				// (Charging up ExampleWhipProjectileAdvanced will increase the number of segments.)
				else if (i > 2 * (totalSegments / 3)) {  // At 2/3 of the way across the whip, the third segment is used.
					// Third segment
					frame.Y = 58;
					frame.Height = 16;
				}
				else if (i > totalSegments / 3) { // At 1/3 of the way across the whip, the second segment is used.
					// Second Segment
					frame.Y = 42;
					frame.Height = 16;
				}
				else {  // At the start of the whip after the handle, the first segment is used.
					// First Segment
					frame.Y = 26;
					frame.Height = 16;
				}

				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;

				float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
				Color color = Lighting.GetColor(element.ToTileCoordinates());

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

				pos += diff;
			}
			return false;
		}
	}
}
