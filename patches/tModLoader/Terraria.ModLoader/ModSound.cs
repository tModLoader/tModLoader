using Microsoft.Xna.Framework.Audio;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class allows you to customize how a sound you add is played. To use this, pass an instance to Mod.AddSound, or if you are autoloading sounds, give an overriding class the same name as the file with the sound you are adding.
	/// </summary>
	public class ModSound
	{
		/// <summary>
		/// The SoundEffect instance of the sound that this ModSound controls. This is here so you can call CreateInstance on it.
		/// </summary>
		public SoundEffect sound {
			get;
			internal set;
		}

		/// <summary>
		/// Override this hook to customize how this sound is played. If this sound is already currently playing, you have the option to call soundInstance.Stop(). You must eventually assign the result of sound.CreateInstance() to soundInstace. Afterwards, you can modify soundInstance.Volume, soundInstance.Pan, and soundInstance.Pitch to your liking. The default volume and pan have been passed as parameters. Volume measures loudness, pan measures how far to the left or right the sound is, and pitch measures the octave. Finally, call Main.PlaySoundInstance(soundInstance).
		/// </summary>
		public virtual SoundEffectInstance PlaySound(ref SoundEffectInstance soundInstance, float volume, float pan, SoundType type) {
			return soundInstance;
		}
	}
}
