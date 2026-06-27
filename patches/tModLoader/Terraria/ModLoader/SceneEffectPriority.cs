namespace Terraria.ModLoader;

/// <summary>
/// This enum dictates from low to high which SceneEffect selections take priority.
/// Setting appropriate priority values is important so that your mod works well with other mods and vanilla selections.
/// </summary>
public enum SceneEffectPriority
{
	/// <summary>Represents no priority</summary>
	None,
	/// <summary> Will override vanilla SceneEffect for Hallow, Ocean, Desert, Overworld, Night</summary>
	BiomeLow,
	/// <summary> Will override vanilla SceneEffect for Meteor, Jungle, Graveyard, Snow</summary>
	BiomeMedium,
	/// <summary> Will override vanilla SceneEffect for Temple, Dungeon, Mushrooms, Corruption, Crimson</summary>
	BiomeHigh,
	/// <summary> Will override vanilla SceneEffect for Sandstorm, Hell, Above surface during Eclipse, Space, Shimmer</summary>
	Environment,
	/// <summary> Will override vanilla SceneEffect for Pirate Invasion, Goblin Invasion, Old Ones Army, Pumpkin Moon, Frost Moon</summary>
	Event,
	/// <summary>All other bosses and default modded boss priority</summary>
	BossLow,
	/// <summary>Will override vanilla SceneEffect for Martian Madness, Celestial Towers, Plantera, Mechdusa</summary>
	BossMedium,
	/// <summary>Will override SceneEffect of Moon Lord and Torch God</summary>
	BossHigh
}
