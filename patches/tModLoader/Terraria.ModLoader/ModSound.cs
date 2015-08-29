using System;
using Microsoft.Xna.Framework.Audio;
using Terraria;

namespace Terraria.ModLoader
{
	public class ModSound
	{
		public SoundEffect sound
		{
			get;
			internal set;
		}

		public virtual void PlaySound(ref SoundEffectInstance soundInstance, float volume, float pan, SoundType type)
		{
		}
	}
}
