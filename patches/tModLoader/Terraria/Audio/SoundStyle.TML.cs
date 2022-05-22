using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;
using System;
using Terraria.ModLoader;
using Terraria.Utilities;

#nullable enable

namespace Terraria.Audio
{
	// Completely reimplemented by TML.
	public record struct SoundStyle
	{
		private const float MinPitchValue = -1f;
		private const float MaxPitchValue = 1f;

		private static readonly UnifiedRandom Random = new();

		private int[]? variants;
		private float volume = 1f;
		private float pitch = 0f;
		private float pitchVariance = 0f;
		private Asset<SoundEffect>? effectCache = null;
		private Asset<SoundEffect>?[]? variantsEffectCache = null;

		public string SoundPath { get; set; }
		public SoundType Type { get; set; }
		public string? Group { get; set; } = null;

		public int MaxInstances { get; set; } = 1;
		public bool RestartIfPlaying { get; set; } = true;
		public bool PlayOnlyIfFocused { get; set; } = false;
		
		//TODO: Behavior to be implemented, and in some other way, as this is, questionable.
		internal bool UsesMusicPitch { get; set; } = false;

		public Span<int> Variants {
			get => variants;
			set {
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				if (value.Length == 0)
					throw new ArgumentException("Styles array must not be empty.", nameof(value));

				Array.Resize(ref variants, value.Length);

				value.CopyTo(variants);
			}
		}

		public float Volume {
			get => volume;
			set => volume = MathHelper.Clamp(volume, 0f, 1f);
		}

		public float Pitch {
			get => pitch;
			set => pitch = MathHelper.Clamp(pitch, MinPitchValue, MaxPitchValue);
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

		public SoundStyle(string soundPath, SoundType type = SoundType.Sound) {
			SoundPath = soundPath;
			Type = type;
			variants = null;
		}

		public SoundStyle(string soundPath, int numVariants, SoundType type = SoundType.Sound) : this(soundPath, type) {
			if (numVariants > 1) {
				variants = CreateVariants(1, numVariants);
			}
		}

		public SoundStyle(string soundPath, int variantSuffixesStart, int numVariants, SoundType type = SoundType.Sound) : this(soundPath, type) {
			if (numVariants > 1) {
				variants = CreateVariants(variantSuffixesStart, numVariants);
			}
		}

		public SoundStyle(string soundPath, ReadOnlySpan<int> variantSuffixes, SoundType type = SoundType.Sound) : this(soundPath, type) {
			if (variantSuffixes.Length == 0)
				throw new ArgumentException("At least one style must be provided.", nameof(variantSuffixes));

			variants = variantSuffixes.ToArray();
		}

		public bool IsTheSameAs(SoundStyle style) {
			if (Group != null && Group == style.Group)
				return true;

			if (SoundPath == style.SoundPath)
				return true;

			return false;
		}

		public SoundEffect GetRandomSound() {
			Asset<SoundEffect> asset;

			if (variants == null || variants.Length == 0) {
				asset = effectCache ??= ModContent.Request<SoundEffect>(SoundPath, AssetRequestMode.ImmediateLoad);
			}
			else {
				int variantId = Main.rand.Next(variants.Length);
				int variant = variants[variantId];

				Array.Resize(ref variantsEffectCache, variants.Length);

				asset = variantsEffectCache[variantId] ??= ModContent.Request<SoundEffect>(SoundPath + variant, AssetRequestMode.ImmediateLoad);
			}

			return asset.Value;
		}

		public float GetRandomPitch()
			=> MathHelper.Clamp(Pitch + ((Random.NextFloat() - 0.5f) * PitchVariance), MinPitchValue, MaxPitchValue);

		internal SoundStyle WithVolume(float volume)
			=> this with { Volume = volume };
		
		internal SoundStyle WithPitchVariance(float pitchVariance)
			=> this with { PitchVariance = pitchVariance };

		private static int[] CreateVariants(int start, int count) {
			if (count <= 1)
				return Array.Empty<int>();

			int[] result = new int[count];

			for (int i = 0; i < count; i++) {
				result[i] = start + i;
			}

			return result;
		}
	}
}
