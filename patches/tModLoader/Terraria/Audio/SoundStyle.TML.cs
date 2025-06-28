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

/// <inheritdoc cref="SoundStyle.SoundLimitBehavior"/>
public enum SoundLimitBehavior
{
	/// <summary> When the sound limit is reached, no sound instance will be started. </summary>
	IgnoreNew,
	/// <summary> When the sound limit is reached, a currently playing sound will be stopped and a new sound instance will be started. </summary>
	ReplaceOldest,
}

/// <inheritdoc cref="SoundStyle.PauseBehavior"/>
public enum PauseBehavior
{
	/// <summary> This sound will keep playing even when the game is paused. </summary>
	KeepPlaying,
	/// <summary> This sound will pause when the game is paused or unfocused and resume once the game is resumed. </summary>
	PauseWithGame,
	/// <summary> This sound will stop when the game is paused or unfocused. </summary>
	StopWhenGamePaused,
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
	private int rerollAttempts = 0;

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
	/// <br/><br/> If using variants, use <see cref="LimitsArePerVariant"/> to allow <see cref="MaxInstances"/> to apply to each variant individually rather than to all variants as a group.
	/// <br/><br/> Set to 0 for no limits.
	/// </summary>
	public int MaxInstances { get; set; } = 1;

	/// <summary>
	/// Determines what the action taken when the max amount of sound instances is reached.
	/// <br/><br/> Defaults to <see cref="SoundLimitBehavior.ReplaceOldest"/>, which means a currently playing sound will be stopped and a new sound instance will be started.
	/// </summary>
	public SoundLimitBehavior SoundLimitBehavior { get; set; } = SoundLimitBehavior.ReplaceOldest;

	/// <summary>
	/// How many additional times to attempt to find a variant that is not currently playing before applying the SoundLimitBehavior. Only has effect if LimitsArePerVariant is true. Defaults to 0.
	/// </summary>
	public int RerollAttempts {
		get => rerollAttempts;
		set => rerollAttempts = Math.Max(0, value);
	}

	/// <summary>
	/// If true, then variants are treated as different sounds for the purposes of <see cref="SoundLimitBehavior"/> and <see cref="MaxInstances"/>. Defaults to false, meaning that all variants share the same sound instance limitations.
	/// </summary>
	public bool LimitsArePerVariant { get; set; } = false;

	/// <summary> If true, this sound won't play if the game's window isn't selected. </summary>
	public bool PlayOnlyIfFocused { get; set; } = false;

	/// <summary>
	/// Determines how the sound will be affected when the game is paused (or unfocused) and subsequently resumed. Long-running sounds might benefit from changing this value.
	/// <br/><br/> Defaults to <see cref="PauseBehavior.KeepPlaying"/>, which means the sound will continue playing while the game is paused.
	/// </summary>
	public PauseBehavior PauseBehavior { get; set; } = PauseBehavior.KeepPlaying;

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

	internal int? SelectedVariant { get; set; }

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
			float halfVariance = PitchVariance / 2;
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
	/// <summary>
	/// Checks if this SoundStyle is the same as another SoundStyle. This method takes into account differences in chosen variants if <see cref="LimitsArePerVariant"/> is true.
	/// </summary>
	public bool IsTheSameAs(SoundStyle style)
	{
		if (LimitsArePerVariant && SelectedVariant != style.SelectedVariant)
			return false;

		if (Identifier != null || style.Identifier != null)
			return Identifier == style.Identifier;

		if (SoundPath == style.SoundPath)
			return true;

		return false;
	}

	/// <summary>
	/// Same as <see cref="IsTheSameAs(SoundStyle)"/> except it doesn't take into account differences in chosen variants.
	/// </summary>
	public bool IsVariantOf(SoundStyle style)
	{
		if (Identifier != null || style.Identifier != null)
			return Identifier == style.Identifier;

		if (SoundPath == style.SoundPath)
			return true;

		return false;
	}

	[Obsolete("Renamed to GetSoundEffect")]
	public SoundEffect GetRandomSound() => GetSoundEffect();

	public SoundEffect GetSoundEffect()
	{
		Asset<SoundEffect> asset;

		if (variants == null || variants.Length == 0) {
			asset = effectCache ??= ModContent.Request<SoundEffect>(SoundPath, AssetRequestMode.ImmediateLoad);
		}
		else {
			int variantIndex = SelectedVariant ?? GetRandomVariantIndex();
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

	internal SoundStyle WithSelectedVariant(int? random = null)
	{
		if (variants == null || variants.Length == 0)
			return this;
		return this with { SelectedVariant = random ?? GetRandomVariantIndex() };
	}

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
