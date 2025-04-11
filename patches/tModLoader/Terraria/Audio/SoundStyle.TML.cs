using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;
using System;
using System.Linq;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Terraria.Audio.ActiveSound;

#nullable enable

// Completely reimplemented by TML.

namespace Terraria.Audio;

public enum SoundLimitBehavior
{
	IgnoreNew,
	ReplaceOldest,
}

/// <summary>
/// This data type describes in detail how a sound should be played.
/// <br/> Passable to the <see cref="SoundEngine.PlaySound(in SoundStyle, Vector2?, SoundUpdateCallback?)"/> method.
/// </summary>
public record struct SoundStyle
{
	private static readonly UnifiedRandom Random = new();

	private int[]? variants;
	private float[]? variantsWeights = null;
	private float? totalVariantWeight = null;
	private float volume = 1f;
	private float pitch = 0f;
	private float pitchVariance = 0f;
	private Asset<SoundEffect>? effectCache = null;
	private Asset<SoundEffect>?[]? variantsEffectCache = null;

	/// <summary> The sound effect to play. </summary>
	public string SoundPath { get; set; }

	/// <summary>
	/// Controls which volume setting will this be affected by.
	/// <br/> Ambience sounds also don't play when the game is out of focus.
	/// </summary>
	public SoundType Type { get; set; }

	/// <summary> If defined, this string will be the only thing used to determine which styles should instances be shared with. </summary>
	public string? Identifier { get; set; } = null;

	/// <summary>
	/// The max amount of sound instances that this style will allow creating, before stopping a playing sound or refusing to play a new one.
	/// <br/> Set to 0 for no limits.
	/// </summary>
	public int MaxInstances { get; set; } = 1;

	/// <summary> Determines what the action taken when the max amount of sound instances is reached. </summary>
	public SoundLimitBehavior SoundLimitBehavior { get; set; } = SoundLimitBehavior.ReplaceOldest;

	/// <summary> If true, this sound won't play if the game's window isn't selected. </summary>
	public bool PlayOnlyIfFocused { get; set; } = false;

	/// <summary> Whether or not to loop played sounds. </summary>
	public bool IsLooped { get; set; } = false;

	/// <summary>
	/// Whether or not this sound obeys the <see cref="Main.musicPitch"/> field to decide its pitch.<br/>
	/// Defaults to false. Used in vanilla by the sounds for the Bell, the (Magical) Harp, and The Axe.<br/>
	/// Could prove useful, but is kept internal for the moment.
	/// </summary>
	internal bool UsesMusicPitch { get; set; } = false;

	/// <summary>
	/// An array of possible suffixes to randomly append to after <see cref="SoundPath"/>.
	/// <br/> Setting this property resets <see cref="VariantsWeights"/>.
	/// </summary>
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

	/// <summary>
	/// An array of randomization weights to optionally go with <see cref="Variants"/>.
	/// <br/> Set this last, if at all, as the <see cref="Variants"/>'s setter resets all weights data.
	/// </summary>
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

	/// <summary> The volume multiplier to play sounds with. </summary>
	public float Volume {
		get => volume;
		set => volume = MathHelper.Clamp(value, 0f, 1f);
	}

	/// <summary>
	/// The pitch <b>offset</b> to play sounds with.
	/// <para/>In XNA and FNA, Pitch ranges from -1.0f (down one octave) to 1.0f (up one octave). 0.0f is unity (normal) pitch.
	/// </summary>
	public float Pitch {
		get => pitch;
		set => pitch = value;
	}

	/// <summary>
	/// The pitch offset randomness value. Cannot be negative.
	/// <br/>With Pitch at 0.0, and PitchVariance at 1.0, used pitch will range from -0.5 to 0.5. 
	/// <para/>In XNA and FNA, Pitch ranges from -1.0f (down one octave) to 1.0f (up one octave). 0.0f is unity (normal) pitch.
	/// </summary>
	public float PitchVariance {
		get => pitchVariance;
		set {
			if (value < 0f)
				throw new ArgumentException("Pitch variance cannot be negative.", nameof(value));

			pitchVariance = value;
		}
	}

	/// <summary>
	/// A helper property for controlling both Pitch and PitchVariance at once.
	/// <para/>In XNA and FNA, Pitch ranges from -1.0f (down one octave) to 1.0f (up one octave). 0.0f is unity (normal) pitch.
	/// </summary>
	public (float minPitch, float maxPitch) PitchRange {
		get {
			float halfVariance = PitchVariance;
			float minPitch = Pitch - halfVariance;
			float maxPitch = Pitch + halfVariance;

			return (minPitch, maxPitch);
		}
		set {
			if (value.minPitch > value.maxPitch)
				throw new ArgumentException("Min pitch cannot be greater than max pitch.", nameof(value));
			
			Pitch = (value.minPitch + value.maxPitch) * 0.5f;
			PitchVariance = value.maxPitch - value.minPitch;
		}
	}

	public SoundStyle(string soundPath, SoundType type = SoundType.Sound)
	{
		SoundPath = soundPath;
		Type = type;
		variants = null;
	}

	public SoundStyle(string soundPath, int numVariants, SoundType type = SoundType.Sound) : this(soundPath, type)
	{
		if (numVariants > 1) {
			variants = CreateVariants(1, numVariants);
		}
	}

	public SoundStyle(string soundPath, int variantSuffixesStart, int numVariants, SoundType type = SoundType.Sound) : this(soundPath, type)
	{
		if (numVariants > 1) {
			variants = CreateVariants(variantSuffixesStart, numVariants);
		}
	}

	public SoundStyle(string soundPath, ReadOnlySpan<int> variants, SoundType type = SoundType.Sound) : this(soundPath, type)
	{
		this.variants = variants.IsEmpty ? null : variants.ToArray();
	}

	public SoundStyle(string soundPath, ReadOnlySpan<(int variant, float weight)> weightedVariants, SoundType type = SoundType.Sound) : this(soundPath, type)
	{
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
	public bool IsTheSameAs(SoundStyle style)
	{
		if (Identifier != null && Identifier == style.Identifier)
			return true;

		if (SoundPath == style.SoundPath)
			return true;

		return false;
	}

	public SoundEffect GetRandomSound()
	{
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
		=> Pitch + ((Random.NextFloat() - 0.5f) * PitchVariance);

	internal SoundStyle WithVolume(float volume)
		=> this with { Volume = volume };
	
	internal SoundStyle WithPitchVariance(float pitchVariance)
		=> this with { PitchVariance = pitchVariance };

	public SoundStyle WithVolumeScale(float scale)
		=> this with { Volume = Volume * scale };

	public SoundStyle WithPitchOffset(float offset)
		=> this with { Pitch = Pitch + offset };

	private int GetRandomVariantIndex()
	{
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

	private static int[] CreateVariants(int start, int count)
	{
		if (count <= 1)
			return Array.Empty<int>();

		int[] result = new int[count];

		for (int i = 0; i < count; i++) {
			result[i] = start + i;
		}

		return result;
	}
}
