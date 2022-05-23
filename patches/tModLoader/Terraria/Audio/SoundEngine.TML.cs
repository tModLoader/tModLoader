using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ReLogic.Utilities;
using Terraria.ID;
using Terraria.ModLoader;

#nullable enable

namespace Terraria.Audio
{
	partial class SoundEngine
	{
		// Public API methods

		/// <summary>
		/// Attempts to play a sound, and returns a valid <see cref="SlotId"/> handle to it on success.
		/// </summary>
		/// <param name="style"> The sound style that describes everything about the played sound. </param>
		/// <param name="position"> An optional 2D position to play the sound at. When null, this sound will be heard everywhere. </param>
		public static SlotId PlaySound(in SoundStyle style, Vector2? position = null) {
			if (Main.dedServ || !IsAudioSupported) {
				return SlotId.Invalid;
			}

			return SoundPlayer.Play(in style, position);
		}

		/// <inheritdoc cref="SoundPlayer.TryGetActiveSound(SlotId, out ActiveSound?)"/>
		public static bool TryGetActiveSound(SlotId slotId, out ActiveSound? result) {
			if (Main.dedServ || !IsAudioSupported) {
				result = null;
				return false;
			}

			return SoundPlayer.TryGetActiveSound(slotId, out result);
		}

		// Internal redirects

		internal static SoundEffectInstance? PlaySound(SoundStyle? style, Vector2? position = null) {
			if (style == null)
				return null;
			
			var slotId = PlaySound(style.Value, position);

			return slotId.IsValid ? GetActiveSound(slotId)?.Sound : null;
		}

		internal static SoundEffectInstance? PlaySound(SoundStyle? type, int x, int y) //(SoundStyle type, int x = -1, int y = -1)
			=> PlaySound(type, XYToOptionalPosition(x, y));

		internal static void PlaySound(int type, Vector2 position, int style = 1)
			=> PlaySound(type, (int)position.X, (int)position.Y, style);

		internal static SoundEffectInstance? PlaySound(int type, int x = -1, int y = -1, int Style = 1, float volumeScale = 1f, float pitchOffset = 0f) {
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
			=> PlaySound(in style, position);

		internal static ActiveSound? GetActiveSound(SlotId slotId)
			=> TryGetActiveSound(slotId, out var result) ? result : null;

		// Utilities

		private static Vector2? XYToOptionalPosition(int x, int y)
			=> x != -1 || y != -1 ? new Vector2(x, y) : null;
	}
}
