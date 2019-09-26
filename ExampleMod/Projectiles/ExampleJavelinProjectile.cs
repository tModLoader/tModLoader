using ExampleMod.Dusts;
using ExampleMod.Items.Weapons;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Projectiles
{
	public class ExampleJavelinProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Javelin");
		}

		public override void SetDefaults() {
			projectile.width = 16;
			projectile.height = 16;
			projectile.aiStyle = -1;
			projectile.friendly = true;
			projectile.melee = true;
			projectile.penetrate = 3;
			projectile.hide = true;
		}

		// See ExampleBehindTilesProjectile. 
		public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI) {
			// If attached to an NPC, draw behind tiles (and the npc) if that NPC is behind tiles, otherwise just behind the NPC.
			if (projectile.ai[0] == 1f) // or if(isStickingToTarget) since we made that helper method.
			{
				int npcIndex = (int)projectile.ai[1];
				if (npcIndex >= 0 && npcIndex < 200 && Main.npc[npcIndex].active) {
					if (Main.npc[npcIndex].behindTiles) {
						drawCacheProjsBehindNPCsAndTiles.Add(index);
					}
					else {
						drawCacheProjsBehindNPCs.Add(index);
					}

					return;
				}
			}
			// Since we aren't attached, add to this list
			drawCacheProjsBehindProjectiles.Add(index);
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
			// For going through platforms and such, javelins use a tad smaller size
			width = height = 10; // notice we set the width to the height, the height to 10. so both are 10
			return true;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			// Inflate some target hitboxes if they are beyond 8,8 size
			if (targetHitbox.Width > 8 && targetHitbox.Height > 8) {
				targetHitbox.Inflate(-targetHitbox.Width / 8, -targetHitbox.Height / 8);
			}
			// Return if the hitboxes intersects, which means the javelin collides or not
			return projHitbox.Intersects(targetHitbox);
		}

		public override void Kill(int timeLeft) {
			Main.PlaySound(0, (int)projectile.position.X, (int)projectile.position.Y); // Play a death sound
			Vector2 usePos = projectile.position; // Position to use for dusts
			
			// Please note the usage of MathHelper, please use this!
			// We subtract 90 degrees as radians to the rotation vector to offset the sprite as its default rotation in the sprite isn't aligned properly.
			Vector2 rotVector = (projectile.rotation - MathHelper.ToRadians(90f)).ToRotationVector2(); // rotation vector to use for dust velocity
			usePos += rotVector * 16f;

			// Declaring a constant in-line is fine as it will be optimized by the compiler
			// It is however recommended to define it outside method scope if used elswhere as well
			// They are useful to make numbers that don't change more descriptive
			const int NUM_DUSTS = 20;

			// Spawn some dusts upon javelin death
			for (int i = 0; i < NUM_DUSTS; i++) {
				// Create a new dust
				Dust dust = Dust.NewDustDirect(usePos, projectile.width, projectile.height, 81);
				dust.position = (dust.position + projectile.Center) / 2f;
				dust.velocity += rotVector * 2f;
				dust.velocity *= 0.5f;
				dust.noGravity = true;
				usePos -= rotVector * 8f;
			}

			// Make sure to only spawn items if you are the projectile owner.
			// This is an important check as Kill() is called on clients, and you only want the item to drop once
			if (projectile.owner == Main.myPlayer) {
				// Drop a javelin item, 1 in 18 chance (~5.5% chance)
				int item =
				Main.rand.NextBool(18)
					? Item.NewItem(projectile.getRect(), ItemType<ExampleJavelin>())
					: 0;

				// Sync the drop for multiplayer
				// Note the usage of Terraria.ID.MessageID, please use this!
				if (Main.netMode == 1 && item >= 0) {
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
				}
			}
		}

		// 
		/*
		 * The following showcases recommended practice to work with the ai field
		 * You make a property that uses the ai as backing field
		 * This allows you to contextualize ai better in the code
		 */

		// Are we sticking to a target?
		public bool IsStickingToTarget {
			get => projectile.ai[0] == 1f;
			set => projectile.ai[0] = value ? 1f : 0f;
		}

		// Index of the current target
		public int TargetWhoAmI {
			get => (int)projectile.ai[1];
			set => projectile.ai[1] = value;
		}

		private const int MAX_STICKY_JAVELINS = 6; // This is the max. amount of javelins being able to attach
		private readonly Point[] _stickingJavelins = new Point[MAX_STICKY_JAVELINS]; // The point array holding for sticking javelins

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			IsStickingToTarget = true; // we are sticking to a target
			TargetWhoAmI = target.whoAmI; // Set the target whoAmI
			projectile.velocity =
				(target.Center - projectile.Center) *
				0.75f; // Change velocity based on delta center of targets (difference between entity centers)
			projectile.netUpdate = true; // netUpdate this javelin
			target.AddBuff(BuffType<Buffs.ExampleJavelin>(), 900); // Adds the ExampleJavelin debuff for a very small DoT

			projectile.damage = 0; // Makes sure the sticking javelins do not deal damage anymore

			// It is recommended to split your code into separate methods to keep code clean and clear
			UpdateStickyJavelins(target);
		}

		/*
		 * The following code handles the javelin sticking to the enemy hit.
		 */
		private void UpdateStickyJavelins(NPC target)
		{
			int currentJavelinIndex = 0; // The javelin index

			for (int i = 0; i < Main.maxProjectiles; i++) // Loop all projectiles
			{
				Projectile currentProjectile = Main.projectile[i];
				if (i != projectile.whoAmI // Make sure the looped projectile is not the current javelin
				    && currentProjectile.active // Make sure the projectile is active
				    && currentProjectile.owner == Main.myPlayer // Make sure the projectile's owner is the client's player
				    && currentProjectile.type == projectile.type // Make sure the projectile is of the same type as this javelin
				    && currentProjectile.modProjectile is ExampleJavelinProjectile javelinProjectile // Use a pattern match cast so we can access the projectile like an ExampleJavelinProjectile
				    && javelinProjectile.IsStickingToTarget // the previous pattern match allows us to use our properties
				    && javelinProjectile.TargetWhoAmI == target.whoAmI) {

					_stickingJavelins[currentJavelinIndex++] = new Point(i, currentProjectile.timeLeft); // Add the current projectile's index and timeleft to the point array
					if (currentJavelinIndex >= _stickingJavelins.Length)  // If the javelin's index is bigger than or equal to the point array's length, break
						break;
				}
			}

			// Remove the oldest sticky javelin if we exceeded the maximum
			if (currentJavelinIndex >= MAX_STICKY_JAVELINS)
			{
				int oldJavelinIndex = 0;
				// Loop our point array
				for (int i = 1; i < MAX_STICKY_JAVELINS; i++) {
					// Remove the already existing javelin if it's timeLeft value (which is the Y value in our point array) is smaller than the new javelin's timeLeft
					if (_stickingJavelins[i].Y < _stickingJavelins[oldJavelinIndex].Y) {
						oldJavelinIndex = i; // Remember the index of the removed javelin
					}
				}
				// Remember that the X value in our point array was equal to the index of that javelin, so it's used here to kill it.
				Main.projectile[_stickingJavelins[oldJavelinIndex].X].Kill();
			}
		}

		// Added these 2 constant to showcase how you could make AI code cleaner by doing this
		// Change this number if you want to alter how long the javelin can travel at a constant speed
		private const int MAX_TICKS = 45;

		// Change this number if you want to alter how the alpha changes
		private const int ALPHA_REDUCTION = 25;

		public override void AI() {

			UpdateAlpha();
			// Run either the Sticky AI or Normal AI
			// Separating into different methods helps keeps your AI clean
			if (IsStickingToTarget) StickyAI();
			else NormalAI();
		}

		private void UpdateAlpha()
		{
			// Slowly remove alpha as it is present
			if (projectile.alpha > 0) {
				projectile.alpha -= ALPHA_REDUCTION;
			}

			// If alpha gets lower than 0, set it to 0
			if (projectile.alpha < 0) {
				projectile.alpha = 0;
			}
		}

		private void NormalAI()
		{
			TargetWhoAmI++;

			// For a little while, the javelin will travel with the same speed, but after this, the javelin drops velocity very quickly.
			if (TargetWhoAmI >= MAX_TICKS) {
				// Change these multiplication factors to alter the javelin's movement change after reaching maxTicks
				const float velXmult = 0.98f; // x velocity factor, every AI update the x velocity will be 98% of the original speed
				const float velYmult = 0.35f; // y velocity factor, every AI update the y velocity will be be 0.35f bigger of the original speed, causing the javelin to drop to the ground
				TargetWhoAmI = MAX_TICKS; // set ai1 to maxTicks continuously
				projectile.velocity.X *= velXmult;
				projectile.velocity.Y += velYmult;
			}

			// Make sure to set the rotation accordingly to the velocity, and add some to work around the sprite's rotation
			// Please notice the MathHelper usage, offset the rotation by 90 degrees (to radians because rotation uses radians) because the sprite's rotation is not aligned!
			projectile.rotation =
				projectile.velocity.ToRotation() +
				MathHelper.ToRadians(90f); 

			// Spawn some random dusts as the javelin travels
			if (Main.rand.NextBool(3)) {
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.height, projectile.width, DustType<Sparkle>(),
					projectile.velocity.X * .2f, projectile.velocity.Y * .2f, 200, Scale: 1.2f);
				dust.velocity += projectile.velocity * 0.3f;
				dust.velocity *= 0.2f;
			}
			if (Main.rand.NextBool(4)) {
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.height, projectile.width, DustType<Sparkle>(),
					0, 0, 254, Scale: 0.3f);
				dust.velocity += projectile.velocity * 0.5f;
				dust.velocity *= 0.5f;
			}
		}

		private void StickyAI()
		{
			// These 2 could probably be moved to the ModifyNPCHit hook, but in vanilla they are present in the AI
			projectile.ignoreWater = true; // Make sure the projectile ignores water
			projectile.tileCollide = false; // Make sure the projectile doesn't collide with tiles anymore
			const int aiFactor = 15; // Change this factor to change the 'lifetime' of this sticking javelin
			projectile.localAI[0] += 1f;

			// Every 30 ticks, the javelin will perform a hit effect
			bool hitEffect = projectile.localAI[0] % 30f == 0f;
			int projTargetIndex = (int)TargetWhoAmI;
			if (projectile.localAI[0] >= 60 * aiFactor || projTargetIndex < 0 || projTargetIndex >= 200) { // If the index is past its limits, kill it
				projectile.Kill();
			}
			else if (Main.npc[projTargetIndex].active && !Main.npc[projTargetIndex].dontTakeDamage) { // If the target is active and can take damage
				// Set the projectile's position relative to the target's center
				projectile.Center = Main.npc[projTargetIndex].Center - projectile.velocity * 2f;
				projectile.gfxOffY = Main.npc[projTargetIndex].gfxOffY;
				if (hitEffect) { // Perform a hit effect here
					Main.npc[projTargetIndex].HitEffect(0, 1.0);
				}
			}
			else { // Otherwise, kill the projectile
				projectile.Kill();
			}
		}
	}
}
