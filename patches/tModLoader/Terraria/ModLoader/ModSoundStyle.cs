using Microsoft.Xna.Framework.Audio;
using System.IO;
using Terraria.Audio;

namespace Terraria.ModLoader
{
	public class ModSoundStyle : SoundStyle
	{
		public readonly string ModName;
		public readonly string SoundPath;
		public readonly int Variations;

		private string pathWithoutExtension;
		private string extension;

		public override bool IsTrackable => true;

		public ModSoundStyle(string soundPath, int variations = 0, SoundType type = SoundType.Sound, float volume = 1.0f, float pitch = 0f, float pitchVariance = 0f) : base(volume, pitch, pitchVariance, type) {
			ModContent.SplitName(soundPath, out ModName, out SoundPath);

			Variations = variations;

			Setup();
		}

		public ModSoundStyle(string modName, string soundPath, int variations = 0, SoundType type = SoundType.Sound, float volume = 1.0f, float pitch = 0f, float pitchVariance = 0f) : base(volume, pitch, pitchVariance, type) {
			ModName = modName;
			SoundPath = soundPath;
			Variations = variations;

			Setup();
		}

		public override SoundEffect GetRandomSound() {
			string path = Variations == 0 ? SoundPath : pathWithoutExtension + (Main.rand.Next(Variations) + 1) + extension;

			return SoundLoader.GetSound(ModName, path).Value;
		}

		private void Setup() {
			extension = Path.GetExtension(SoundPath);
			pathWithoutExtension = Path.ChangeExtension(SoundPath, null);
		}
	}
}
