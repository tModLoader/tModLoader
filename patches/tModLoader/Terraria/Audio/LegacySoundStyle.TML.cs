using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using Terraria.Utilities;

#nullable enable

namespace Terraria.Audio
{
	public record struct LegacySoundStyle : ISoundStyle
	{
		private const float MinPitchValue = -1f;
		private const float MaxPitchValue = 1f;

		private static readonly UnifiedRandom Random = new();

		private int[] styles;
		private float pitchVariance = 0f;

		public int SoundId { get; set; }
		public SoundType Type { get; set; }
		public float Volume { get; set; } = 1f;
		public float Pitch { get; set; } = 0f;

		//TODO: Behavior to be implemented: [[
		public int MaxInstances { get; set; } = 1;
		public bool RestartIfPlaying { get; set; } = true;
		// (Internal ones are questionable)
		internal bool UsesMusicPitch { get; set; } = false;
		internal bool PlayOnlyIfFocused { get; set; } = false;
		// ]]

		public int Style {
			set {
				Array.Resize(ref styles, 1);
				
				styles[0] = value;
			}
		}

		public Span<int> Styles {
			get => styles;
			set {
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				if (value.Length == 0)
					throw new ArgumentException("Styles array must not be empty.", nameof(value));

				Array.Resize(ref styles, value.Length);

				value.CopyTo(styles);
			}
		}

		public float PitchVariance {
			get => pitchVariance;
			set {
				if (pitchVariance < 0f)
					throw new ArgumentException("Pitch variance cannot be negative.", nameof(value));

				pitchVariance = value;
			}
		}

		public (float minPitch, float maxPitch) PitchRange {
			get {
				float halfVariance = PitchVariance;
				float minPitch = Math.Max(MinPitchValue, Pitch - halfVariance);
				float maxPitch = Math.Min(MaxPitchValue, Pitch + halfVariance);

				return (minPitch, maxPitch);
			}
			set {
				float minPitch = value.minPitch;
				float maxPitch = value.maxPitch;

				if (minPitch > maxPitch)
					throw new ArgumentException("Min pitch cannot be greater than max pitch.", nameof(value));
				
				minPitch = Math.Max(MinPitchValue, minPitch);
				maxPitch = Math.Min(MaxPitchValue, maxPitch);

				Pitch = (minPitch + maxPitch) * 0.5f;
				PitchVariance = maxPitch - minPitch;
			}
		}

		public LegacySoundStyle(int soundId, int style = 0, SoundType type = SoundType.Sound)
			: this(soundId, stackalloc int[] { style }, type) { }
		
		public LegacySoundStyle(int soundId, ReadOnlySpan<int> styles, SoundType type = SoundType.Sound) {
			if (styles.Length == 0)
				throw new ArgumentException("At least one style must be provided.", nameof(styles));

			SoundId = soundId;
			this.styles = styles.ToArray();
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

		/*
		public bool Includes(int soundId, int style) {
			if (SoundId == soundId && style >= Style)
				return style < Style + Variations;

			return false;
		}
		*/

		public SoundEffect GetRandomSound()
			=> SoundEngine.GetTrackableSoundByStyleId(SoundId, GetRandomStyle());

		public float GetRandomPitch()
			=> MathHelper.Clamp(Pitch + ((Random.NextFloat() - 0.5f) * PitchVariance), -1f, 1f);

		internal int GetRandomStyle() {
			if (Styles == null || Styles.Length == 0)
				return 0;

			if (Styles.Length == 1)
				return Styles[0];

			return Styles[Random.Next(Styles.Length)];
		}

		public static bool operator ==(ISoundStyle soundStyleA, LegacySoundStyle soundStyleB) => Equals(soundStyleA, soundStyleB);
		
		public static bool operator !=(ISoundStyle soundStyleA, LegacySoundStyle soundStyleB) => !Equals(soundStyleA, soundStyleB);
	}
}
