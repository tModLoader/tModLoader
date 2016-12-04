using System;

namespace Terraria.ModLoader
{
	/*
	 *  in Vanilla SoundID.cs
		public const int Item = 2;
		public const int NPCHit = 3;
		public const int NPCKilled = 4;
	*/
	public enum SoundType
	{
		Item = 2,
		NPCHit = 3,
		NPCKilled = 4,
		Custom = 50,
		Music = 51
	}
}
