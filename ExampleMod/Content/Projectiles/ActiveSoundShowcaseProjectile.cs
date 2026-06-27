using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	/// <summary>
	/// Showcases ActiveSounds. The various styles when studied in order serve to teach important concepts, please experiment with them in-game.
	/// Do note that the earlier examples aren't useful to copy, if you are just looking for an example to copy, consult SoundUpdateCallbackApproach and LoopedSound, as they are the most suitable examples.
	/// This example serves as a companion to the Active Sounds section of the Sounds wiki page, please study them both together: https://github.com/tModLoader/tModLoader/wiki/Basic-Sounds#active-sounds
	/// </summary>
	public class ActiveSoundShowcaseProjectile : ModProjectile
	{
		internal enum ActiveSoundShowcaseStyle
		{
			// This example plays a long sound (12 seconds) and never attempts to change it. Notice how the sound plays without any location, the player and projectile can move left or right and the sound panning and volume do not change. Also note that the sound keeps playing after the projectile dies.
			FireAndForget,
			// This example improves on FireAndForget. The Projectile position is passed into PlaySound. The sound still does not update location, but the player can move around the initial spawn location and the sound pans and volume adjusts accordingly.
			FireAndForgetPlusInitialPosition,
			// Further improving on FireAndForgetPlusInitialPosition, this example updates the sound location in AI and stops the sound when the projectile is killed in Kill.
			SyncSoundToProjectilePosition,
			// Further improving on SyncSoundToProjectilePosition, this example uses the SoundUpdateCallback parameter to keep all sound logic organized in a single place instead of spread between different methods.
			SoundUpdateCallbackApproach,
			// LoopedSound shows using SoundUpdateCallback once again to adjust sound position. The SoundStyle used is looped, so SoundUpdateCallback is necessary in case Projectile.Kill doesn't get called for some exceptional reason.
			LoopedSound,
			// LoopedSoundAdvanced adjusts pitch and volume dynamically in the SoundUpdateCallback, in addition to the usual sound position.
			LoopedSoundAdvanced,
		}

		private ActiveSoundShowcaseStyle Style {
			get => (ActiveSoundShowcaseStyle)Projectile.ai[0];
			set => Projectile.ai[0] = (float)value;
		}

		SlotId soundSlot;
		bool played = false;

		SoundStyle soundStyleTwister = new SoundStyle("Terraria/Sounds/Custom/dd2_book_staff_twister_loop") {
			PauseBehavior = PauseBehavior.PauseWithGame, // Rather than keep playing, this sound pauses when the game is paused.
		};

		SoundStyle soundStyleIgniteLoop = new SoundStyle("Terraria/Sounds/Custom/dd2_kobold_ignite_loop") {
			IsLooped = true,
			PauseBehavior = PauseBehavior.StopWhenGamePaused, // Note that the code examples using this SoundStyle automatically start this sound once more when the game is un-paused.
			SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
			// Note that MaxInstances defaults to 1.
		};

		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 6;
		}

		public override void SetDefaults() {
			Projectile.width = 22;
			Projectile.height = 24;
			Projectile.penetrate = 4; // Can bounce 3 times and dies on 4th tile collide
			Projectile.timeLeft = 300; // Despawns after 5 seconds
		}

		public override void OnSpawn(IEntitySource source) {
			Main.NewText($"{Style}");
		}

		public override void AI() {
			Projectile.frame = (int)Style;

			// By default (in single player) sounds continue playing when the game is paused or loses focus (player switches to another program). In some situations the modder might want to pause and resume a sound when the game pauses and resumes, in other situations the modder might just want to stop the sound when the game pauses. These behaviors can be implemented by setting SoundStyle.PauseBehavior to PauseBehavior.PauseWithGame or PauseBehavior.StopWhenGamePaused. The 2 SoundStyles in this example use each of these options because the sound clips are quite long.
			// In some situations the modder might want to restart a sound when the game is focused again, in other situations that might not be desired. Some of these examples use a bool, "played", to track if the sound has been played since the projectile spawned, while others do not and will attempt to restart the sound if it is not currently playing.

			// Also note that in this example the SoundStyle all have "MaxInstances = 1" and "SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest" by default, so if 2 projectiles attempt to play the same sound, they'll constantly interrupt each other every AI update, making a horrible sound.
			// In a real mod, the modder should design the SoundStyle properties and PlaySound logic to meet their needs. For example, the modder might decide that 3 overlapping sounds is too chaotic and adjust MaxInstances accordingly. The modder might also decide that the sound should not restart when the game is re-focused and use logic to only attempt to play the sound once.
			switch (Style) {
				case ActiveSoundShowcaseStyle.FireAndForget:
					if (!played) {
						played = true;
						SoundEngine.PlaySound(soundStyleTwister);
					}
					break;
				case ActiveSoundShowcaseStyle.FireAndForgetPlusInitialPosition:
					if (!played) {
						played = true;
						SoundEngine.PlaySound(soundStyleTwister, Projectile.position);
					}
					break;
				case ActiveSoundShowcaseStyle.SyncSoundToProjectilePosition:
					if (!SoundEngine.TryGetActiveSound(soundSlot, out var activeSoundTwister)) {
						soundSlot = SoundEngine.PlaySound(soundStyleTwister, Projectile.position);
					}
					else {
						// If the sound is playing, update the sound's position to match the current position of the projectile.
						activeSoundTwister.Position = Projectile.position;
					}
					break;
				case ActiveSoundShowcaseStyle.SoundUpdateCallbackApproach:
					if (!SoundEngine.TryGetActiveSound(soundSlot, out var _)) {
						var tracker = new ProjectileAudioTracker(Projectile);
						soundSlot = SoundEngine.PlaySound(soundStyleTwister, Projectile.position, soundInstance => BasicSoundUpdateCallback(tracker, soundInstance));

						// If only the sound stopping when the projectile is killed is required, this simpler code can be used:
						//soundSlot = SoundEngine.PlaySound(soundStyleTwister, Projectile.position, soundInstance => tracker.IsActiveAndInGame());

						// Do NOT make this mistake, the ProjectileAudioTracker object must be initialized outside the callback:
						// soundSlot = SoundEngine.PlaySound(soundStyleTwister, Projectile.position, soundInstance => new ProjectileAudioTracker(Projectile).IsActiveAndInGame()); // WRONG
					}
					break;
				case ActiveSoundShowcaseStyle.LoopedSound:
					if (!SoundEngine.TryGetActiveSound(soundSlot, out var _)) {
						var tracker = new ProjectileAudioTracker(Projectile);
						soundSlot = SoundEngine.PlaySound(soundStyleIgniteLoop, Projectile.position, soundInstance => {
							// The SoundUpdateCallback can be inlined if desired, such as in this example. Otherwise, LoopedSoundAdvanced shows the other approach
							soundInstance.Position = Projectile.position;
							return tracker.IsActiveAndInGame();
						});
					}

					// SlotId can be stored as a float, such as in Projectile.localAI entries. This can be an alternative to making a SlotId field in the class.
					// Don't use ai slots for SlotId, since those will sync and sounds and sound slots are completely local and are not synced
					// SlotId soundSlot = SlotId.FromFloat(Projectile.localAI[0]);
					// Projectile.localAI[0] = soundSlot.ToFloat();

					// As an alternate approach to TryGetActiveSound, we could use FindActiveSound. The difference is that FindActiveSound will find any ActiveSound matching the given SoundStyle, so if 2 projectile instances spawn the same SoundStyle, the ActiveSound retrieved isn't necessarily the sound spawned by this instance. This can be useful, but in this situation we want the ActiveSound spawned by this projectile.
					/*
					var activeSoundB = SoundEngine.FindActiveSound(soundStyleIgniteLoop);
					if (activeSoundB == null) {
						SoundEngine.PlaySound(soundStyleIgniteLoop, Projectile.position, updateIgniteLoop);
					}
					*/
					break;
				case ActiveSoundShowcaseStyle.LoopedSoundAdvanced:
					if (!SoundEngine.TryGetActiveSound(soundSlot, out var _)) {
						var tracker = new ProjectileAudioTracker(Projectile);
						soundSlot = SoundEngine.PlaySound(soundStyleIgniteLoop, Projectile.position, soundInstance => AdvancedSoundUpdateCallback(tracker, soundInstance));
					}
					break;
			}
		}

		private bool BasicSoundUpdateCallback(ProjectileAudioTracker tracker, ActiveSound soundInstance) {
			// Update sound location according to projectile position
			soundInstance.Position = Projectile.position;
			// ProjectileAudioTracker is necessary to avoid rare situations where sounds can loop indefinitely. IsActiveAndInGame returns a value indicating if the sound should still be active.
			return tracker.IsActiveAndInGame();
		}

		private bool AdvancedSoundUpdateCallback(ProjectileAudioTracker tracker, ActiveSound soundInstance) {
			soundInstance.Position = Projectile.position;

			// Dynamic pitch example: Pitch rises each time the projectile bounces
			soundInstance.Pitch = (Projectile.maxPenetrate - Projectile.penetrate) * 0.15f;

			// Muffle the sound if the projectile is wet
			if (Projectile.wet) {
				soundInstance.Pitch -= 0.4f;
				soundInstance.Volume = MathHelper.Clamp(soundInstance.Style.Volume - 0.4f, 0f, 1f);
			}

			return tracker.IsActiveAndInGame();
		}

		public override void OnKill(int timeLeft) {
			// For long sounds, the sound can be stopped when the projectile is killed.
			// This approach is not foolproof, so it should NOT be used, especially for looped sounds.
			// See SoundUpdateCallbackApproach for the better approach. This example, however, does show how an ActiveSound can be modified from another hook other than where the sound was played.
			if (Style == ActiveSoundShowcaseStyle.SyncSoundToProjectilePosition) {
				if (SoundEngine.TryGetActiveSound(soundSlot, out var activeSound)) {
					activeSound.Stop();
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			// If collide with tile, reduce the penetrate.
			// So the projectile can reflect at most 3 times
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
