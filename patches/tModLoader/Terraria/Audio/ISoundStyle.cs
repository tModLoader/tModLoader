using Microsoft.Xna.Framework.Audio;

namespace Terraria.Audio
{
	//TML: SoundStyle class got replaced with this interface, to turn sound styles into structures
	public interface ISoundStyle
	{
		float Volume { get; }
		SoundType Type { get; }

		float GetRandomPitch();
		SoundEffect GetRandomSound();
	}
}
