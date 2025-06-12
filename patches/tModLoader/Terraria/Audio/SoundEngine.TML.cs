using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ReLogic.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using Terraria.ID;
using Terraria.ModLoader;

#nullable enable

namespace Terraria.Audio;

partial class SoundEngine
{
	// Public API methods

	/// <inheritdoc cref="PlaySound(in SoundStyle, Vector2?, SoundUpdateCallback?)" />
	/// <summary>
	/// Attempts to play a sound style with the provided sound style (if it's not null), and returns a valid <see cref="SlotId"/> handle to it on success.
	/// </summary>
	public static SlotId PlaySound(in SoundStyle? style, Vector2? position = null, SoundUpdateCallback? updateCallback = null)
	{
		if (!style.HasValue) {
			return SlotId.Invalid;
		}

		return PlaySound(style.Value, position, updateCallback);
	}

	/// <summary>
	/// Attempts to play a sound with the provided sound style, and returns a valid <see cref="SlotId"/> handle to it on success.
	/// </summary>
	/// <param name="style"> The sound style that describes everything about the played sound. </param>
	/// <param name="position"> An optional 2D position to play the sound at. When null, this sound will be heard everywhere. </param>
	/// <param name="updateCallback"> A callback for customizing the behavior of the created sound instance, like tying its existence to a projectile using <see cref="ProjectileAudioTracker"/>. </param>
	public static SlotId PlaySound(in SoundStyle style, Vector2? position = null, SoundUpdateCallback? updateCallback = null)
	{
		if (Main.dedServ || !IsAudioSupported) {
			return SlotId.Invalid;
		}

		return SoundPlayer.Play(in style, position, updateCallback);
	}

	/// <inheritdoc cref="SoundPlayer.TryGetActiveSound(SlotId, out ActiveSound?)"/>
	public static bool TryGetActiveSound(SlotId slotId, [NotNullWhen(true)] out ActiveSound? result)
	{
		if (Main.dedServ || !IsAudioSupported) {
			result = null;
			return false;
		}

		return SoundPlayer.TryGetActiveSound(slotId, out result);
	}

	// Internal redirects

	internal static SoundEffectInstance? PlaySound(SoundStyle? style, Vector2? position = null)
	{
		var slotId = PlaySound(in style, position);

		return slotId.IsValid ? GetActiveSound(slotId)?.Sound : null;
	}

	internal static SoundEffectInstance? PlaySound(SoundStyle? type, int x, int y) //(SoundStyle type, int x = -1, int y = -1)
		=> PlaySound(type, XYToOptionalPosition(x, y));

	internal static void PlaySound(int type, Vector2 position, int style = 1)
		=> PlaySound(type, (int)position.X, (int)position.Y, style);

	internal static SoundEffectInstance? PlaySound(int type, int x = -1, int y = -1, int Style = 1, float volumeScale = 1f, float pitchOffset = 0f)
	{
		if (!SoundID.TryGetLegacyStyle(type, Style, out var soundStyle)) {
			Logging.tML.Warn($"Failed to get legacy sound style for ({type}, {Style}) input.");

			return null;
		}

		soundStyle = soundStyle with {
			Volume = soundStyle.Volume * volumeScale,
			Pitch = soundStyle.Pitch + pitchOffset
		};

		var slotId = PlaySound(soundStyle, XYToOptionalPosition(x, y));

		return slotId.IsValid ? GetActiveSound(slotId)?.Sound : null;
	}

	internal static SlotId PlayTrackedSound(in SoundStyle style, Vector2? position = null)
		=> PlaySound(style with { PauseBehavior = PauseBehavior.StopWhenGamePaused }, position);

	internal static SlotId PlayTrackedLoopedSound(in SoundStyle style, Vector2 position, Func<bool>? loopingCondition = null)
		=> PlaySound(style with { PauseBehavior = PauseBehavior.StopWhenGamePaused }, (Vector2?)position, (SoundUpdateCallback)(_ => loopingCondition()));

	internal static ActiveSound? GetActiveSound(SlotId slotId)
		=> TryGetActiveSound(slotId, out var result) ? result : null;

	// Utilities

	private static Vector2? XYToOptionalPosition(int x, int y)
		=> x != -1 || y != -1 ? new Vector2(x, y) : null;
}
