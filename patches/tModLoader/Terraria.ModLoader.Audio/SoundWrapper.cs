using Microsoft.Xna.Framework.Audio;

namespace Terraria.ModLoader
{
	public class SoundWrapper
	{
		public SoundEffect soundEffect;
		public SoundMP3 mp3Instance;

		public static implicit operator SoundWrapper(SoundEffect that){return new SoundWrapper{soundEffect=that};}
		public static implicit operator SoundWrapper(SoundMP3 that){return new SoundWrapper{mp3Instance=that};}
		//public static implicit operator SoundEffect(SoundEffectWrapper that){return that.soundEffect;}

		public SoundEffectInstance CreateInstance()
		{
			if(soundEffect!=null){return soundEffect.CreateInstance();}
			if(mp3Instance!=null){return mp3Instance.CreateInstance();}
			return null;
		}
	}
}
