using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using System;
using System.Diagnostics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	// WIP. Need more complete examples. Add once orphan sound issue is fixed.
	public class ActiveSoundShowcaseProjectile : ModProjectile
	{
		private enum ActiveSoundShowcaseStyle
		{
			A,
			B,
			C,
			D,
			E,
			F,
		}

		private ActiveSoundShowcaseStyle Style {
			get => (ActiveSoundShowcaseStyle)Projectile.ai[0];
			set => Projectile.ai[0] = (float)value;
		}

		int StateTimer;
		SlotId soundSlot;
		SlotId soundSlot2;

		// buggy, doesn't seem to pause consistently when losing focus, when looped, won't ever go away.
		SoundStyle soundStyleA = new SoundStyle("Terraria/Sounds/Custom/dd2_kobold_ignite_loop") {
			IsLooped = true,
			//PlayOnlyIfFocused = true,
			//	MaxInstances = 1,
			//	SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
		};

		// need to show SlotId.FromFloat and ToFloat

		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 6;
		}

		public override void SetDefaults() {
			Projectile.width = 22;
			Projectile.height = 24;
			//Projectile.tileCollide = false;
			Projectile.penetrate = 4; // Can bounce 4 times

			Projectile.timeLeft = 300; // Despawns after 5 seconds
		}

		public override void OnSpawn(IEntitySource source) {
			Main.NewText($"{Style}");
		}

		public override void AI() {
			Projectile.frame = (int)Style;

			StateTimer++;
			//if (StateTimer % 20 == 0) {
				switch (Style) {
					case ActiveSoundShowcaseStyle.A:
						if (!SoundEngine.TryGetActiveSound(soundSlot, out var activeSound)) {
							soundSlot = SoundEngine.PlaySound(soundStyleA, Projectile.position, updateIgniteLoop);
						}
						break;
					case ActiveSoundShowcaseStyle.B:
						// Using FindActiveSound, we find 
						var activeSoundB = SoundEngine.FindActiveSound(soundStyleA);
						if (activeSoundB != null) {
							SoundEngine.PlaySound(soundStyleA, Projectile.position, updateIgniteLoop);
						}
						break;
					case ActiveSoundShowcaseStyle.C:
						// Don't use ai slots for SlotId, since those will sync and sounds and sound slots are completlly local and are not synced
						SlotId slotId = SlotId.FromFloat(Projectile.localAI[0]);
						if (!SoundEngine.TryGetActiveSound(slotId, out var activeSoundC)) {
							//slotId = SoundEngine.PlaySound(soundStyleA, Projectile.position, updateIgniteLoop);

							var tracker = new ProjectileAudioTracker(Projectile);
							slotId = SoundEngine.PlaySound(soundStyleA, Projectile.position, sound => updateIgniteLoop2(tracker, sound));

							// kobolds sound weird...did I mess up tmod code?

							Projectile.localAI[0] = slotId.ToFloat();
						}
						break;
				}
			//}
			return;



			base.AI();
			StateTimer++;
			if(StateTimer == 120) {

			}

			bool a = soundSlot.IsValid;

			// Explain how to be defensive. Sound might be paused by alttabbing out.
			// Use IsActive?

			if (StateTimer % 20 == 0) {
				SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_kobold_ignite_loop") { 
					IsLooped = true,
				//	MaxInstances = 1,
				//	SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
				};

				// how to handle multiple of this projectile? They both attempt to restart
				// Use 
				var rttt = SoundEngine.FindActiveSound(style);


				if (!SoundEngine.TryGetActiveSound(soundSlot, out var activeSound)) {
					/*
					var tracker = new ProjectileAudioTracker(Projectile);
					soundSlot = SoundEngine.PlaySound(style, Projectile.position, (SoundUpdateCallback)(_ => tracker.IsActiveAndInGame()));

					soundSlot = SoundEngine.PlaySound(style, Projectile.position, sound => updateIgniteLoop2(tracker, sound));

					// buggy?
					soundSlot = SoundEngine.PlaySound(style, Projectile.position, (SoundUpdateCallback)(_ => new ProjectileAudioTracker(Projectile).IsActiveAndInGame()));
					*/

					soundSlot = SoundEngine.PlaySound(style, Projectile.position, updateIgniteLoop);
				}
				else {
					// do nothing.
				}
			}
		}
		private bool updateIgniteLoop2(ProjectileAudioTracker tracker, ActiveSound soundInstance) {

			return tracker.IsActiveAndInGame();
		}

		private bool updateIgniteLoop(ActiveSound soundInstance) {
			soundInstance.Position = Projectile.position;

			if (Projectile.ModProjectile != this) {
				// replaced with another projectile? Same type still possibly?

				// looks like only when a new projectile spawns will Projectile.ModProjectile update.
			}

			// apply some conditional?
			//soundInstance.Pitch = 0;
			//if (Main.rand.NextBool(100)) {
			//	soundInstance.Pitch = Main.rand.NextBool() ? 0.5f : -0.5f;
			//}

			// Dynamic pitch example: Pitch rises each time the projectile bounces
			soundInstance.Pitch = (Projectile.maxPenetrate - Projectile.penetrate) * 0.15f;

			// Issue, when projectile despawns, does old modprojectile still point to projectile in main.projectile?
			// SHould CleanupModReferences clean up modprojectile references? Dpes it already?

			bool alive = !Main.gameMenu && Projectile.active;
			if (!alive) {

			}
			return alive;
		}

		public override void Kill(int timeLeft) {
			return;
			bool a = soundSlot.IsValid;
			if (SoundEngine.TryGetActiveSound(soundSlot, out var activeSound)) {
				activeSound.Stop();
			}
			bool b = soundSlot.IsValid;
			// What does IsActive even mean? Why is TryGetActiveSound returning true after Stop?

			if (SoundEngine.TryGetActiveSound(soundSlot, out var activeSound2)) {
				activeSound2.Stop();
			}

			//SoundEngine.FindActiveSound
		}


		public override bool OnTileCollide(Vector2 oldVelocity) {
			// If collide with tile, reduce the penetrate.
			// So the projectile can reflect at most 5 times
			Projectile.penetrate--;
			if (Projectile.penetrate <= 0) {
				Projectile.Kill();
			}
			else {
				Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
				SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

				// If the projectile hits the left or right side of the tile, reverse the X velocity
				if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon) {
					Projectile.velocity.X = -oldVelocity.X;
				}

				// If the projectile hits the top or bottom side of the tile, reverse the Y velocity
				if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon) {
					Projectile.velocity.Y = -oldVelocity.Y;
				}
			}

			return false;
		}
	}
}
