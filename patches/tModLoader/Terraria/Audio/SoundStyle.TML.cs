using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;
using System;
using System.Linq;
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
		private float[]? variantsWeights = null;
		private float? totalVariantWeight = null;
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
		
		// Questionable workaround for old music instruments.
		internal bool UsesMusicPitch { get; set; } = false;

		public ReadOnlySpan<int> Variants {
			get => variants;
			set {
				variantsWeights = null;
				totalVariantWeight = null;

				if (value.IsEmpty) {
					variants = null;
					return;
				}

				variants = value.ToArray();
			}
		}
		
		public ReadOnlySpan<float> VariantsWeights {
			get => variantsWeights;
			set {
				if (value.Length == 0) {
					variantsWeights = null;
					totalVariantWeight = null;
					return;
				}

				if (variants == null)
					throw new ArgumentException("Variants weights must be set after variants.");

				if (value.Length != variants.Length)
					throw new ArgumentException("Variants and their weights must have the same length.");

				variantsWeights = value.ToArray();
				totalVariantWeight = null;
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

		public SoundStyle(string soundPath, ReadOnlySpan<int> variants, SoundType type = SoundType.Sound) : this(soundPath, type) {
			this.variants = variants.IsEmpty ? null : variants.ToArray();
		}

		public SoundStyle(string soundPath, ReadOnlySpan<(int variant, float weight)> weightedVariants, SoundType type = SoundType.Sound) : this(soundPath, type) {
			if (weightedVariants.IsEmpty) {
				variants = null;
				return;
			}

			variants = new int[weightedVariants.Length];
			variantsWeights = new float[weightedVariants.Length];

			for (int i = 0; i < weightedVariants.Length; i++) {
				(int variant, float weight) = weightedVariants[i];

				variants[i] = variant;
				variantsWeights[i] = weight;
			}
		}

		// To be optimized, improved.
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
				int variantIndex = GetRandomVariantIndex();
				int variant = variants[variantIndex];

				Array.Resize(ref variantsEffectCache, variants.Length);

				asset = variantsEffectCache[variantIndex] ??= ModContent.Request<SoundEffect>(SoundPath + variant, AssetRequestMode.ImmediateLoad);
			}

			return asset.Value;
		}

		public float GetRandomPitch()
			=> MathHelper.Clamp(Pitch + ((Random.NextFloat() - 0.5f) * PitchVariance), MinPitchValue, MaxPitchValue);

		internal SoundStyle WithVolume(float volume)
			=> this with { Volume = volume };
		
		internal SoundStyle WithPitchVariance(float pitchVariance)
			=> this with { PitchVariance = pitchVariance };

		private int GetRandomVariantIndex() {
			if (variantsWeights == null) {
				// Simple random.
				return Random.Next(variants!.Length);
			}
			
			// Weighted random.
			totalVariantWeight ??= variantsWeights.Sum();

			float random = (float)Random.NextDouble() * totalVariantWeight.Value;
			float accumulatedWeight = 0f;

			for (int i = 0; i < variantsWeights.Length; i++) {
				accumulatedWeight += variantsWeights[i];

				if (random < accumulatedWeight) {
					return i;
				}
			}

			return 0; // Unreachable.
		}

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
