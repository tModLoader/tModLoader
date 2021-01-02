using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Terraria.Audio
{
	partial class LegacySoundStyle
	{
		public override SoundEffectInstance Play(Vector2? position, float volumeScale)
			=> SoundEngine.PlaySound(SoundId, (int)(position?.X ?? -1f), (int)(position?.Y ?? -1f), Style, Volume * volumeScale, GetRandomPitch());
	}
}
