using Microsoft.Xna.Framework.Audio;

namespace Terraria.ModLoader
{
	public class SoundWrapper
	{
		public SoundEffect soundWAV;
		public SoundMP3 soundMP3;

		public bool IsWAV { get { return soundWAV != null; } }
		public bool IsMP3 { get { return soundMP3 != null; } }

		public static implicit operator SoundWrapper(SoundEffect that) { return new SoundWrapper { soundWAV = that }; }
		public static implicit operator SoundWrapper(SoundMP3 that) { return new SoundWrapper { soundMP3 = that }; }

		public SoundEffectInstance CreateInstance()
		{
			if (IsWAV) { return soundWAV.CreateInstance(); }
			if (IsMP3) { return soundMP3.CreateInstance(); }
			return null;
		}
	}
}
