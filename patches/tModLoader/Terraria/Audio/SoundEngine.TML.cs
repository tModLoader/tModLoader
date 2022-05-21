using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ReLogic.Utilities;
using Terraria.ID;

#nullable enable

namespace Terraria.Audio
{
	partial class SoundEngine
	{
		public static SoundEffectInstance? PlaySound(ISoundStyle type, Vector2? position = null) {
			if (type == null)
				return null;

			SlotId slot = PlayTrackedSound(type, position);

			return slot.IsValid ? GetActiveSound(slot)?.Sound : null;
		}

		// Internal redirects

		internal static SoundEffectInstance? PlaySound(SoundStyle type, int x = -1, int y = -1)
			=> PlaySound(type, XYToOptionalPosition(x, y));

		internal static void PlaySound(int type, Vector2 position, int style = 1)
			=> PlaySound(type, (int)position.X, (int)position.Y, style);

		internal static SoundEffectInstance? PlaySound(int type, int x = -1, int y = -1, int Style = 1, float volumeScale = 1f, float pitchOffset = 0f) {
			var soundStyle = SoundID.GetLegacyStyle(type, Style);

			return PlaySound(soundStyle, XYToOptionalPosition(x, y));
		}

		// Utilities

		private static Vector2? XYToOptionalPosition(int x, int y)
			=> x != -1 || y != -1 ? new Vector2(x, y) : null;
	}
}
