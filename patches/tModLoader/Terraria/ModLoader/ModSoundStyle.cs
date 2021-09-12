using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;
using Terraria.Audio;

namespace Terraria.ModLoader
{
	public struct ModSoundStyle : ISoundStyle
	{
		private Asset<SoundEffect> effect;
		private Asset<SoundEffect>[] variants;

		public string SoundPath { get; set; }
		public SoundType Type { get; set; }
		public int Variations { get; set; }
		public float Volume { get; set; }
		public float Pitch { get; set; }
		public float PitchVariance { get; set; }

		public ModSoundStyle(string soundPath, int variations = 0, SoundType type = SoundType.Sound, float volume = 1.0f, float pitch = 0f, float pitchVariance = 0f) {
			SoundPath = soundPath;
			Variations = variations;
			Type = type;
			Volume = volume;
			Pitch = pitch;
			PitchVariance = pitchVariance;

			effect = null;
			variants = null;

			if (variations > 0)
				variants = new Asset<SoundEffect>[variations];
		}

		public SoundEffect GetRandomSound() {
			Asset<SoundEffect> asset;

			if (Variations == 0) {
				asset = effect ??= ModContent.Request<SoundEffect>(SoundPath, AssetRequestMode.ImmediateLoad);
			}
			else {
				int variant = Main.rand.Next(Variations);
				asset = variants[variant] ??= ModContent.Request<SoundEffect>(SoundPath + (variant + 1), AssetRequestMode.ImmediateLoad);
			}

			return asset.Value;
		}

		public float GetRandomPitch() => MathHelper.Clamp(Pitch + ((Main.rand.NextFloat() - 0.5f) * PitchVariance), -1f, 1f);
	}
}
