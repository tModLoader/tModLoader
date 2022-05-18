using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Terraria.Utilities;

namespace Terraria.Audio
{
	public record struct LegacySoundStyle : ISoundStyle
	{
		private static readonly UnifiedRandom Random = new();

		public int SoundId { get; set; }
		public int Style { get; set; }
		public int Variations { get; set; }
		public SoundType Type { get; set; }
		public float Volume { get; set; } = 1f;
		public float Pitch { get; set; }
		public float PitchVariance { get; set; }

		public LegacySoundStyle(int soundId, int style, SoundType type = SoundType.Sound)
			: this(soundId, style, 1, type) { }

		public LegacySoundStyle(int soundId, int style, int variations, SoundType type = SoundType.Sound) {
			Pitch = 0f;
			PitchVariance = 0f;
			Volume = 1f;

			Style = style;
			Variations = variations;
			SoundId = soundId;
			Type = type;
		}

		public LegacySoundStyle WithVolume(float volume) {
			var result = this;
			
			result.Volume = volume;

			return result;
		}

		public LegacySoundStyle WithPitchVariance(float pitchVariance) {
			var result = this;

			result.PitchVariance = pitchVariance;

			return result;
		}

		public bool Includes(int soundId, int style) {
			if (SoundId == soundId && style >= Style)
				return style < Style + Variations;

			return false;
		}

		public SoundEffect GetRandomSound()
			=> SoundEngine.GetTrackableSoundByStyleId(SoundId, Style + (Variations > 1 ? Random.Next(Variations) : 0));

		public float GetRandomPitch()
			=> MathHelper.Clamp(Pitch + ((Random.NextFloat() - 0.5f) * PitchVariance), -1f, 1f);

		public static bool operator ==(ISoundStyle soundStyleA, LegacySoundStyle soundStyleB) => Equals(soundStyleA, soundStyleB);
		
		public static bool operator !=(ISoundStyle soundStyleA, LegacySoundStyle soundStyleB) => !Equals(soundStyleA, soundStyleB);
	}
}
