namespace Terraria.ModLoader
{
	/// <summary>
	/// This enum dictates from low to high which Avfx selections take priority.
	/// Setting appropriate priority values is important so that your mod works well with other mods and vanilla selections.
	/// </summary>
	public enum AvfxPriority
	{
		/// <summary>Represents no priority</summary>
		None,
		/// <summary> Will override vanilla Avfx for Hallow, Ocean, Desert, Overworld, Night</summary>
		BiomeLow,
		/// <summary> Will override vanilla Avfx for Dungeon, Meteor, Jungle, Snow</summary>
		BiomeMedium,
		/// <summary> Will override vanilla Avfx for Temple, Mushrooms, Corruption, Crimson, </summary>
		BiomeHigh,
		/// <summary> Will override vanilla Avfx for Sandstorm, Hell, Above surface during Eclipse, Space</summary>
		Environment,
		/// <summary> Will override vanilla Avfx for Pirate Invasion, Goblin Invasion, Old Ones Army</summary>
		Event,
		/// <summary>All other bosses and default modded boss priority</summary>
		BossLow,
		/// <summary>Will override vanilla Avfx for Martian Madness, Celestial Towers, Plantera</summary>
		BossMedium,
		/// <summary>Will override Avfx of Moon Lord</summary>
		BossHigh
	}
}
