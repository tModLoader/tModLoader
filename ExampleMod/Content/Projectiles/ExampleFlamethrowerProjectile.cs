using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	// This projectile demonstrates custom flame behavior and drawing based on the vanilla Flamethrower.
	// It shows how to expand the damage hitbox over time, prevent damage through walls, and draw animated flame trails.
	public class ExampleFlamethrowerProjectile : ModProjectile
	{
		private const float FlameLifetime = 72f;

		private ref float FlameAge => ref Projectile.localAI[0];

		public override void SetStaticDefaults() {
			// Vanilla leaves this at 1 and uses a hardcoded value when drawing.
			// Setting this to a proper value makes sure this projectile gets accurately displayed in other mods that allow projectile spawning, for example.
			Main.projFrames[Type] = 7;
		}

		public override void SetDefaults() {
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.alpha = 255;
			Projectile.penetrate = 4;
			Projectile.extraUpdates = 2;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.ArmorPenetration = 15;
		}

		public override void AI() {
			FlameAge += 1f;
			if (FlameAge >= FlameLifetime) {
				Projectile.Kill();
				return;
			}

			if (FlameAge >= 60f) {
				Projectile.velocity *= 0.95f;
			}

			if (!Main.dedServ && FlameAge < 50f && Main.rand.NextFloat() < 0.25f) {
				float spread = Utils.Remap(FlameAge, 0f, FlameLifetime, 0.5f, 1f);
				Vector2 position = Projectile.Center + Main.rand.NextVector2Circular(60f, 60f) * spread;
				Dust dust = Dust.NewDustDirect(position, 4, 4, DustID.SilverFlame, Alpha: 100);
				dust.noGravity = true;
				dust.scale *= 2.25f;
				dust.velocity *= 1.2f;
				dust.velocity += Projectile.velocity * Utils.Remap(FlameAge, 0f, 45f, 1f, 0.1f) * Utils.Remap(FlameAge, 0f, 6f, 0.1f, 1f);
			}

			if (!Main.dedServ && FlameAge >= 50f && Main.rand.NextBool(2)) {
				Vector2 velocity = (Projectile.Center - Main.player[Projectile.owner].Center).SafeNormalize(Vector2.Zero).RotatedByRandom(MathHelper.PiOver4 / 4f) * 7f;
				Dust smoke = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(50f, 50f) - velocity * 2f, 4, 4, DustID.Smoke, Alpha: 150, newColor: new Color(45, 46, 77));
				smoke.noGravity = true;
				smoke.velocity = velocity;
				smoke.scale *= 1.1f + Main.rand.NextFloat() * 0.2f;
				smoke.customData = -0.3f - 0.15f * Main.rand.NextFloat();
			}
		}

		// The following hooks reproduce behavior that vanilla checks by projectile type, rather than by aiStyle.
		public override bool? CanDamage() => FlameAge < 54f ? null : false;

		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			// Grow the damage area without increasing the small hitbox used for movement and tile collision.
			int expansion = (int)Utils.Remap(FlameAge, 0f, FlameLifetime, 10f, 40f);
			hitbox.Inflate(expansion, expansion);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			// The expanded damage area must not hit an enemy through a solid wall.
			return projHitbox.Intersects(targetHitbox) && Collision.CanHit(Projectile.Center, 0, 0, targetHitbox.Center.ToVector2(), 0, 0);
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity = oldVelocity * 0.95f;
			Projectile.position -= Projectile.velocity;
			return false; // Keep the flame alive at the wall instead of destroying it immediately.
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire3, 1200);
			Projectile.damage = (int)(Projectile.damage * 0.85f); // Subsequent targets take less damage from this flame.
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(BuffID.OnFire3, 1200);
			Projectile.damage = (int)(Projectile.damage * 0.85f);
		}

		public override bool PreDraw(Player player, ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			// Use the registered frame count to split the sprite sheet, but select the fourth frame directly.
			Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, 3);
			float age = FlameAge;
			float progress = age / FlameLifetime;
			if (progress >= 1f) {
				return false;
			}

			Color outerFlame = new Color(129, 148, 154, 200);
			Color flameCore = new Color(234, 241, 240, 70);
			Color fadingFlame = new Color(89, 106, 114, 100);
			Color smoke = new Color(45, 46, 77, 100);
			Color color;
			if (progress < 0.1f) {
				color = Color.Lerp(Color.Transparent, outerFlame, Utils.GetLerpValue(0f, 0.1f, progress, true));
			}
			else if (progress < 0.2f) {
				color = Color.Lerp(outerFlame, flameCore, Utils.GetLerpValue(0.1f, 0.2f, progress, true));
			}
			else if (progress < 0.35f) {
				color = flameCore;
			}
			else if (progress < 0.7f) {
				color = Color.Lerp(flameCore, fadingFlame, Utils.GetLerpValue(0.35f, 0.7f, progress, true));
			}
			else if (progress < 0.85f) {
				color = Color.Lerp(fadingFlame, smoke, Utils.GetLerpValue(0.7f, 0.85f, progress, true));
			}
			else {
				color = Color.Lerp(smoke, Color.Transparent, Utils.GetLerpValue(0.85f, 1f, progress, true));
			}

			float fadeOut = Utils.Remap(age, 60f, FlameLifetime, 1f, 0f);
			float trailLength = Math.Min(age, 20f);
			float scale = Utils.Remap(progress, 0.2f, 0.5f, 0.25f, 1f);
			float step = age > 50f ? 0.175f : 0.2f;
			for (int layer = 0; layer < 2; layer++) {
				for (float offset = 1f; offset >= 0f; offset -= step) {
					float opacity = (1f - offset) * Utils.Remap(progress, 0f, 0.2f, 0f, 1f);
					Vector2 position = Projectile.Center - Main.screenPosition - Projectile.velocity * trailLength * offset;
					Color drawColor = color * opacity;
					Color outerColor = drawColor;
					outerColor.R = (byte)(outerColor.R * 0.65f);
					outerColor.G = (byte)(outerColor.G * 0.75f);
					outerColor.A = (byte)Math.Min(drawColor.A + 80f * opacity, 255f);
					float spin = Main.GlobalTimeWrappedHourly * (offset + 1f) / step * 2f;
					float rotation = Projectile.rotation + offset * MathHelper.PiOver2 + spin;
					float reverseRotation = Projectile.rotation - offset * MathHelper.PiOver2 - spin;
					Vector2 origin = frame.Size() / 2f;
					if (layer == 0) {
						Main.EntitySpriteDraw(texture, position - Projectile.velocity * trailLength * step * 0.5f, frame, outerColor * fadeOut * 0.25f, rotation + MathHelper.PiOver4, origin, scale, SpriteEffects.None);
						Main.EntitySpriteDraw(texture, position, frame, outerColor * fadeOut, reverseRotation, origin, scale, SpriteEffects.None);
					}
					else {
						Main.EntitySpriteDraw(texture, position - Projectile.velocity * trailLength * step * 0.2f, frame, drawColor * fadeOut * 0.25f, rotation + MathHelper.PiOver2, origin, scale * 0.75f, SpriteEffects.None);
						Main.EntitySpriteDraw(texture, position, frame, drawColor * fadeOut, reverseRotation + MathHelper.PiOver2, origin, scale * 0.75f, SpriteEffects.None);
					}
				}
			}

			return false;
		}
	}
}
