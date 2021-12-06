using Terraria.ModLoader;

namespace ExampleMod.Common
{
	// An example ILoadable showing off manual music loading.
	// Manual loading is rarely needed, as, by default, TML will autoload every .wav, .ogg and .mp3 sound file in a 'Music' folder (including sub-directories) as a music track.
	public sealed class ManualMusicRegistrationExample : ILoadable
	{
		public void Load(Mod mod) {
			// When registering music manually, you will have to provide an instance to your mod.
			// Since you're providing an instance of your mod, you should not start the path with your mod's name.
			// Accepted music formats are: .mp3, .ogg, and .wav files.
			// Do NOT add the file extension in your code when adding music!

			// MusicLoader.AddMusic(Mod, "Assets/Music/MysteriousMystery");

			// An example of registration of Music Boxes can be found in 'Content/Items/Placeable/ExampleMusicBox.cs'.
		}

		public void Unload() { }
	}
}
