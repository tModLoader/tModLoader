namespace Terraria.ModLoader
{
	/// <summary>
	/// This enum dictates from low to high which music selections take priority. 
	/// Setting appropriate MusicPriority values in Mod.UpdateMusic is important so that your mod works well with other mods and vanilla music selections.
	/// </summary>
	public enum MusicPriority
	{
		/// <summary>Represents no priority</summary>
		None,
		/// <summary>Hallow, Ocean, Desert, Overworld, Night</summary>
		BiomeLow,
		/// <summary>Dungeon, Meteor, Jungle, Snow</summary>
		BiomeMedium,
		/// <summary>Temple, Mushrooms, Corruption, Crimson, </summary>
		BiomeHigh,
		/// <summary>Sandstorm, Hell, Above surface during Eclipse, Space</summary>
		Environment,
		/// <summary>Pirate Invasion, Goblin Invasion, Old Ones Army</summary>
		Event,
		/// <summary>All other bosses and default modded boss priority</summary>
		BossLow,
		/// <summary>Martian Madness, Celestial Towers, Plantera</summary>
		BossMedium,
		/// <summary>Moon Lord</summary>
		BossHigh
	}
}
