using ExampleMod.Content.Dusts;
using ExampleMod.Content.Items.Weapons;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	// This projectile showcases advanced AI code. Of particular note is a showcase on how projectiles can stick to NPCs in a manner similar to the behavior of vanilla weapons such as Bone Javelin, Daybreak, Blood Butcherer, Stardust Cell Minion, and Tentacle Spike. This code is modeled closely after Bone Javelin.
	public class ExampleJavelinProjectile : ModProjectile
	{
		// These properties wrap the usual ai arrays for cleaner and easier to understand code.
		// Are we sticking to a target?
		public bool IsStickingToTarget {
			get => Projectile.ai[0] == 1f;
			set => Projectile.ai[0] = value ? 1f : 0f;
		}

		// Index of the current target
		public int TargetWhoAmI {
			get => (int)Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		public int GravityDelayTimer {
			get => (int)Projectile.ai[2];
			set => Projectile.ai[2] = value;
		}

		public float StickTimer {
			get => Projectile.localAI[0];
			set => Projectile.localAI[0] = value;
		}

		public override void SetStaticDefaults() {
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.width = 16; // The width of projectile hitbox
			Projectile.height = 16; // The height of projectile hitbox
			Projectile.aiStyle = 0; // The ai style of the projectile (0 means custom AI). For more please reference the source code of Terraria
			Projectile.friendly = true; // Can the projectile deal damage to enemies?
			Projectile.hostile = false; // Can the projectile deal damage to the player?
			Projectile.DamageType = DamageClass.Ranged; // Makes the projectile deal ranged damage. You can set in to DamageClass.Throwing, but that is not used by any vanilla items
			Projectile.penetrate = 2; // How many monsters the projectile can penetrate.
			Projectile.timeLeft = 600; // The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
			Projectile.alpha = 255; // The transparency of the projectile, 255 for completely transparent. Our custom AI below fades our projectile in. Make sure to delete this if you aren't using an aiStyle that fades in.
			Projectile.light = 0.5f; // How much light emit around the projectile
			Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?
			Projectile.tileCollide = true; // Can the projectile collide with tiles?
			Projectile.hide = true; // Makes the projectile completely invisible. We need this to draw our projectile behind enemies/tiles in DrawBehind()
		}

		private const int GravityDelay = 45;

		public override void AI() {
			UpdateAlpha();
			// Run either the Sticky AI or Normal AI
			// Separating into different methods helps keeps your AI clean
			if (IsStickingToTarget) {
				StickyAI();
			}
			else {
				NormalAI();
			}
		}

		private void NormalAI() {
			GravityDelayTimer++; // doesn't make sense.

			// For a little while, the javelin will travel with the same speed, but after this, the javelin drops velocity very quickly.
			if (GravityDelayTimer >= GravityDelay) {
				GravityDelayTimer = GravityDelay;

				// wind resistance
				Projectile.velocity.X *= 0.98f;
				// gravity
				Projectile.velocity.Y += 0.35f;
			}

			// Offset the rotation by 90 degrees because the sprite is oriented vertically.
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);

			// Spawn some random dusts as the javelin travels
			if (Main.rand.NextBool(3)) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, ModContent.DustType<Sparkle>(), Projectile.velocity.X * .2f, Projectile.velocity.Y * .2f, 200, Scale: 1.2f);
				dust.velocity += Projectile.velocity * 0.3f;
				dust.velocity *= 0.2f;
			}
			if (Main.rand.NextBool(4)) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.height, Projectile.width, ModContent.DustType<Sparkle>(),
					0, 0, 254, Scale: 0.3f);
				dust.velocity += Projectile.velocity * 0.5f;
				dust.velocity *= 0.5f;
			}
		}

		private const int StickTime = 60 * 15; // 15 seconds
		private void StickyAI() {
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			StickTimer += 1f;

			// Every 30 ticks, the javelin will perform a hit effect
			bool hitEffect = StickTimer % 30f == 0f;
			int npcTarget = TargetWhoAmI;
			if (StickTimer >= StickTime || npcTarget < 0 || npcTarget >= 200) { // If the index is past its limits, kill it
				Projectile.Kill();
			}
			else if (Main.npc[npcTarget].active && !Main.npc[npcTarget].dontTakeDamage) {
				// If the target is active and can take damage
				// Set the projectile's position relative to the target's center
				Projectile.Center = Main.npc[npcTarget].Center - Projectile.velocity * 2f;
				Projectile.gfxOffY = Main.npc[npcTarget].gfxOffY;
				if (hitEffect) {
					// Perform a hit effect here, causing the npc to react as if hit.
					// Note that this does NOT damage the NPC, the damage is done through the debuff.
					Main.npc[npcTarget].HitEffect(0, 1.0);
				}
			}
			else { // Otherwise, kill the projectile
				Projectile.Kill();
			}
		}

		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position); // Play a death sound
			Vector2 usePos = Projectile.position; // Position to use for dusts

			// Offset the rotation by 90 degrees because the sprite is oriented vertically.
			Vector2 rotationVector = (Projectile.rotation - MathHelper.ToRadians(90f)).ToRotationVector2(); // rotation vector to use for dust velocity
			usePos += rotationVector * 16f;

			// Spawn some dusts upon javelin death
			for (int i = 0; i < 20; i++) {
				// Create a new dust
				Dust dust = Dust.NewDustDirect(usePos, Projectile.width, Projectile.height, DustID.Tin);
				dust.position = (dust.position + Projectile.Center) / 2f;
				dust.velocity += rotationVector * 2f;
				dust.velocity *= 0.5f;
				dust.noGravity = true;
				usePos -= rotationVector * 8f;
			}

			// Make sure to only spawn items if you are the projectile owner.
			// This is an important check as Kill() is called on clients, and you only want the item to drop once
			if (Projectile.owner == Main.myPlayer) {
				// Drop a javelin item, 1 in 18 chance (~5.5% chance)
				int item = 0;
				if (Main.rand.NextBool(18)) {
					item = Item.NewItem(Projectile.GetSource_DropAsItem(), Projectile.getRect(), ModContent.ItemType<ExampleJavelin>());
				}

				// Sync the drop for multiplayer
				// Note the usage of Terraria.ID.MessageID, please use this!
				if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0) {
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
				}
			}
		}

		private const int MaxStickingJavelin = 6; // This is the max amount of javelins able to be attached to a single NPC
		private readonly Point[] stickingJavelins = new Point[MaxStickingJavelin]; // The point array holding for sticking javelins

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			IsStickingToTarget = true; // we are sticking to a target
			TargetWhoAmI = target.whoAmI; // Set the target whoAmI
			Projectile.velocity = (target.Center - Projectile.Center) *
				0.75f; // Change velocity based on delta center of targets (difference between entity centers)
			Projectile.netUpdate = true; // netUpdate this javelin
			Projectile.damage = 0; // Makes sure the sticking javelins do not deal damage anymore

			// ExampleJavelinBuff handles the damage over time (DoT)
			target.AddBuff(ModContent.BuffType<Buffs.ExampleJavelinDebuff>(), 900);

			// KillOldestJavelin will kill the oldest projectile stuck to the specified npc.
			// It only works if ai[0] is 1 when sticking and ai[1] is the target npc index, which is what IsStickingToTarget and TargetWhoAmI correspond to.
			Projectile.KillOldestJavelin(Projectile.whoAmI, Type, target.whoAmI, stickingJavelins);
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			// For going through platforms and such, javelins use a tad smaller size
			width = height = 10; // notice we set the width to the height, the height to 10. so both are 10
			return true;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			// By shrinking target hitboxes by a small amount, this projectile only hits if it more directly hits the target.
			// This helps the javelin stick in a visually appealing place within the target sprite.
			if (targetHitbox.Width > 8 && targetHitbox.Height > 8) {
				targetHitbox.Inflate(-targetHitbox.Width / 8, -targetHitbox.Height / 8);
			}
			// Return if the hitboxes intersects, which means the javelin collides or not
			return projHitbox.Intersects(targetHitbox);
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			// If attached to an NPC, draw behind tiles (and the npc) if that NPC is behind tiles, otherwise just behind the NPC.
			if (IsStickingToTarget) {
				int npcIndex = TargetWhoAmI;
				if (npcIndex >= 0 && npcIndex < 200 && Main.npc[npcIndex].active) {
					if (Main.npc[npcIndex].behindTiles) {
						behindNPCsAndTiles.Add(index);
					}
					else {
						behindNPCsAndTiles.Add(index);
					}

					return;
				}
			}
			// Since we aren't attached, add to this list
			behindNPCsAndTiles.Add(index);
		}

		// Change this number if you want to alter how the alpha changes
		private const int AlphaFadeInSpeed = 25;

		private void UpdateAlpha() {
			// Slowly remove alpha as it is present
			if (Projectile.alpha > 0) {
				Projectile.alpha -= AlphaFadeInSpeed;
			}

			// If alpha gets lower than 0, set it to 0
			if (Projectile.alpha < 0) {
				Projectile.alpha = 0;
			}
		}
	}
}
