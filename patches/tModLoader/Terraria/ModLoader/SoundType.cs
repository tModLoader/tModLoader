namespace Terraria.ModLoader
{
	/*
	 *  in Vanilla SoundID.cs
		public const int Item = 2;
		public const int NPCHit = 3;
		public const int NPCKilled = 4;
	*/
	/// <summary>
	/// This is an enum of the types of sound you can add to the game. This is used for determining whether a sound is a music or a sound effect. If it's a sound effect, it's also used for determining the "type" passed to Main.PlaySound (first parameter) that is required in order for a sound to play; 2 for item sounds, 3 for npcHit sounds, 4 for npcKilled sounds, and SoundLoader.customSoundType for anything else.
	/// </summary>
	public enum SoundType
	{
		Item = 2,
		NPCHit = 3,
		NPCKilled = 4,
		Custom = 50,
		Music = 51
	}
}
