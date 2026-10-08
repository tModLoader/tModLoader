using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	// Here is a simple example on how to change the sound the player makes when taking damage or dying.
	// To make voice item accessories, see Example Voice Accessory.
	// Note: Voice accessories will override what sounds are played here.

	public class ExampleHurtDeathSoundPlayer : ModPlayer {
		public override bool PlayerHurtSound(Vector2 soundPosition) {

			// If the player is riding the Flamingo mount, change their hurt sound to the parrot hurt sound.
			if (Player.mount.Active && Player.mount.Type == MountID.Flamingo) {
				// If the sound is emanating from ourselves (aka local client), soundPosition will be "global" for us so it doesn't pan while we are moving.
				SoundEngine.PlaySound(SoundID.NPCHit46, soundPosition);
				return true; // Return true to prevent vanilla from playing other sounds.
			}
			// If the player is wearing the Moon Lord Mask in the vanity slot.
			if (Player.armor[10].type == ItemID.BossMaskMoonlord) {
				SoundEngine.PlaySound(SoundID.NPCHit57 with { MaxInstances = 3 }, soundPosition);
				return true;
			}

			return false; // Return false to let vanilla play other sounds.
		}

		// Note: The hurt sound will still play in addition to the death sound.
		public override bool PlayerDeathSound() {

			// If the player is riding the Flamingo mount, change their hurt sound to the parrot hurt sound.
			if (Player.mount.Active && Player.mount.Type == MountID.Flamingo) {
				SoundEngine.PlaySound(SoundID.NPCDeath48, Player.position);
				return true;
			}
			// If the player is wearing the Moon Lord Mask in the vanity slot.
			if (Player.armor[10].type == ItemID.BossMaskMoonlord) {
				SoundEngine.PlaySound(SoundID.NPCDeath62, Player.position);
				return true;
			}

			return false;
		}
	}
}
