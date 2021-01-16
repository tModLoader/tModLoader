using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Terraria.Audio
{
	partial class LegacySoundStyle
	{
		public override SoundEffectInstance Play(Vector2? position)
			=> SoundEngine.PlaySound(SoundId, (int)(position?.X ?? -1f), (int)(position?.Y ?? -1f), Style, Volume, GetRandomPitch());
	}
}
