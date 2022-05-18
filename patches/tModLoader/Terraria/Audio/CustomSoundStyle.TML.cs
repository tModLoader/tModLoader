using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Terraria.Utilities;

namespace Terraria.Audio
{
	public record struct CustomSoundStyle : ISoundStyle
	{
		private static readonly UnifiedRandom Random = new();
		private readonly SoundEffect[] _soundEffects;

		public SoundType Type { get; set; }
		public float Volume { get; set; } = 1f;
		public float Pitch { get; set; }
		public float PitchVariance { get; set; }

		public CustomSoundStyle(SoundEffect soundEffect, SoundType type = SoundType.Sound, float volume = 1f, float pitch = 0f, float pitchVariance = 0f)
			: this(new[] { soundEffect }, type, volume, pitch, pitchVariance) { }

		public CustomSoundStyle(SoundEffect[] soundEffects, SoundType type = SoundType.Sound, float volume = 1f, float pitch = 0f, float pitchVariance = 0f) {
			_soundEffects = soundEffects;
			Type = type;
			Volume = volume;
			Pitch = pitch;
			PitchVariance = pitchVariance;
		}

		public SoundEffect GetRandomSound()
			=> _soundEffects[Random.Next(_soundEffects.Length)];

		public float GetRandomPitch()
			=> MathHelper.Clamp(Pitch + ((Random.NextFloat() - 0.5f) * PitchVariance), -1f, 1f);

		public static bool operator ==(ISoundStyle soundStyleA, CustomSoundStyle soundStyleB) => Equals(soundStyleA, soundStyleB);
		
		public static bool operator !=(ISoundStyle soundStyleA, CustomSoundStyle soundStyleB) => !Equals(soundStyleA, soundStyleB);
	}
}
